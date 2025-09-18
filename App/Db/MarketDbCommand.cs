using App.Mapper;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Services.Notification;
using Domain.Interfaces.Services.Order;
using Domain.Models.Api.Order;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Domain.Models.Events;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json;

namespace App.Db;

public class MarketDbCommand(
  P2PDbContext dbContext,
  IChildOffersService childOffers,
  INotificationService notificationService) : IMarketDbCommand
{
  public async Task CreateSellerOfferAsync(OfferInitialized offer)
  {
    try
    {
      var mappedEntity = EscrowOrderMapper.ToEntity(offer);

      mappedEntity.DomainEvents.Add(new OfferCreated(
        Guid.NewGuid(), mappedEntity.Id, mappedEntity.DealId,
        mappedEntity.SellerCrypto, OrderSide.Sell, EventType.OfferCreated));

      await dbContext.EscrowOrders.AddAsync(mappedEntity);
      await dbContext.SaveChangesAsync();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  public async Task CreateBuyerOfferAsync(BuyOrderInitialized offer)
  {
    try
    {
      var mappedEntity = EscrowOrderMapper.ToEntity(offer);

      mappedEntity.DomainEvents.Add(new OfferCreated(
        Guid.NewGuid(), mappedEntity.Id, mappedEntity.DealId,
        mappedEntity.BuyerFiat, OrderSide.Buy, EventType.OfferCreated));

      await dbContext.EscrowOrders.AddAsync(mappedEntity);
      await dbContext.SaveChangesAsync();
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  public async Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto dto)
  {
    var exists = await dbContext.EscrowOrders
      .AsNoTracking()
      .AnyAsync(x => x.DealId == dto.OrderId, CancellationToken.None);
    if (exists)
      throw new InvalidOperationException($"Order with DealId={dto.OrderId} already exists.");

    await using var tx =
      await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, CancellationToken.None);

    var entity = EscrowOrderMapper.ToEntity(dto);
    entity.OfferSide = OrderSide.Buy;
    entity.CreatedAtUtc = DateTime.UtcNow;
    entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

    entity.DomainEvents.Add(new OfferCreated(
      Guid.NewGuid(),
      entity.Id,
      entity.DealId,
      entity.BuyerFiat,
      OrderSide.Buy,
      EventType.OfferCreated));

    await dbContext.EscrowOrders.AddAsync(entity, CancellationToken.None);
    await dbContext.SaveChangesAsync(CancellationToken.None);

    var desiredIds = dto.PaymentMethodIds.Distinct().ToArray();
    var validIds = await dbContext.PaymentMethods
      .Where(pm => desiredIds.Contains(pm.Id))
      .Select(pm => pm.Id)
      .ToListAsync(CancellationToken.None);

    if (validIds.Count != desiredIds.Length)
      throw new ValidationException("Some payment methods don’t exist.");

    var links = validIds.Select(id => new EscrowOrderPaymentMethodEntity
    {
      OrderId = entity.Id,
      MethodId = id
    });

    await dbContext.AddRangeAsync(links, CancellationToken.None);
    await dbContext.SaveChangesAsync(CancellationToken.None);

    await tx.CommitAsync(CancellationToken.None);


    return entity.DealId;
  }

  public async Task UpdateCurrentOfferAsync(UpsertOrderDto upsertOrder)
  {
    //TODO: remove this delay, it's just for testing purposes
    await Task.Delay(4000);

    var entity = await dbContext.EscrowOrders
      .Include(x => x.PaymentMethods)
      .FirstOrDefaultAsync(x => x.DealId == upsertOrder.OrderId);

    if (entity is null)
      throw new InvalidOperationException($"EscrowOrderEntity with DealId {upsertOrder.OrderId} was not found.");

    var previousStatus = entity.EscrowStatus;
    var now = DateTime.UtcNow;
    entity = TrackReleaseTime(entity, previousStatus, upsertOrder.Status, now);


    if (upsertOrder.IsPartial == true)
    {
      var childDto = BuildChildUpsertFrom(upsertOrder, entity);
      var childDtoUpdated = await childOffers.UpsertAsync(childDto, CancellationToken.None);
      if (childDtoUpdated.EscrowStatus is EscrowStatus.SignedByContraAgentSide or EscrowStatus.SignedByOwnerSide)
      {
        await NotifyStatusChange(entity, previousStatus, entity.EscrowStatus);
        return;
      }

      entity.EscrowPda = upsertOrder.EscrowPda ?? entity.EscrowPda;
      entity.IsPartial = true;
      entity.EscrowStatus = upsertOrder.Status ?? entity.EscrowStatus;

      //if (upsertOrder.OrderSide == OrderSide.Sell)
      //{
      entity.SellerCrypto = upsertOrder.Seller ?? entity.SellerCrypto;

      //{
      entity.BuyerFiat = upsertOrder.Buyer ?? entity.BuyerFiat;
      //}

      if (upsertOrder.FilledQuantity.HasValue)
      {
        entity.FilledQuantity += upsertOrder.FilledQuantity.Value;
        TryReleaseParentIfFullyFilled(entity);
      }

      await dbContext.SaveChangesAsync(CancellationToken.None);
      await NotifyStatusChange(entity, previousStatus, entity.EscrowStatus);

      return;
    }

    EscrowOrderPatcher.ApplyUpsert(entity, upsertOrder, MoveToSignedStatus);

    await dbContext.SaveChangesAsync();
    await NotifyStatusChange(entity, previousStatus, entity.EscrowStatus);
  }
  private static EscrowOrderEntity TrackReleaseTime(EscrowOrderEntity entity, EscrowStatus previousStatus, EscrowStatus? newStatus, DateTime now)
  {
    if (newStatus == null) return entity;

    if (previousStatus != EscrowStatus.SignedByOwnerSide &&
        previousStatus != EscrowStatus.SignedByContraAgentSide &&
        (newStatus == EscrowStatus.SignedByOwnerSide || newStatus == EscrowStatus.SignedByContraAgentSide))
    {
      entity.PaymentConfirmedAt = now;
    }

    if ((previousStatus == EscrowStatus.SignedByOwnerSide || previousStatus == EscrowStatus.SignedByContraAgentSide) &&
        newStatus == EscrowStatus.Signed &&
        entity.PaymentConfirmedAt.HasValue)
    {
      entity.CryptoReleasedAt = now;
      entity.ReleaseTimeSeconds = (int)(now - entity.PaymentConfirmedAt.Value).TotalSeconds;
    }

    return entity;
  }

  private EscrowStatus MoveToSignedStatus(EscrowOrderEntity orderEntity, decimal newFilledQ)
  {
    var fromEntity = EscrowOrderDto.FromEntity(orderEntity);
    if (fromEntity.FilledQuantity + newFilledQ >= fromEntity.Amount)
      return EscrowStatus.Signed;

    return orderEntity.EscrowStatus;
  }

  private async Task NotifyStatusChange(EscrowOrderEntity order, EscrowStatus oldStatus, EscrowStatus newStatus)
  {
    if (oldStatus == newStatus) return;

    // Визначаємо кого нотифікувати
    var usersToNotify = new List<string>();
    if (!string.IsNullOrEmpty(order.SellerCrypto)) usersToNotify.Add(order.SellerCrypto);
    if (!string.IsNullOrEmpty(order.BuyerFiat)) usersToNotify.Add(order.BuyerFiat);

    var statusMessage = GetStatusMessage(newStatus);
    var metadata =
      JsonSerializer.Serialize(new { OrderId = order.DealId, OldStatus = oldStatus, NewStatus = newStatus });

    foreach (var userWallet in usersToNotify)
      await notificationService.CreateNotificationAsync(
        userWallet,
        "Order Status Changed",
        $"Your order #{order.DealId} status changed to {statusMessage}",
        NotificationType.OrderStatusChanged,
        order.DealId.ToString(),
        metadata);
  }

  private static string GetStatusMessage(EscrowStatus status)
  {
    return status switch
    {
      EscrowStatus.Signed => "Signed by both parties",
      EscrowStatus.SignedByOwnerSide => "Signed by offer creator",
      EscrowStatus.SignedByContraAgentSide => "Signed by contra agent",
      EscrowStatus.Released => "Trade completed successfully",
      EscrowStatus.Cancelled => "Cancelled",
      EscrowStatus.AdminResolving => "Under admin review",
      _ => status.ToString()
    };
  }

  private static ChildOrderUpsertDto BuildChildUpsertFrom(UpsertOrderDto dto, EscrowOrderEntity parent)
  {
    var ownerWallet = parent.OfferSide == OrderSide.Sell ? parent.SellerCrypto : parent.BuyerFiat;
    var contraWallet = parent.OfferSide == OrderSide.Sell ? dto.Buyer : dto.Seller;

    if (string.IsNullOrWhiteSpace(ownerWallet))
      throw new ArgumentException("Owner wallet is required for child upsert (derived from dto.Seller/dto.Buyer).",
        nameof(dto));
    if (string.IsNullOrWhiteSpace(contraWallet))
      throw new ArgumentException("Contra wallet is required for child upsert (derived from dto.Seller/dto.Buyer).",
        nameof(dto));

    int? childFilled = dto.FilledQuantity.HasValue
      ? decimal.ToInt32(decimal.Round(dto.FilledQuantity.Value, 0, MidpointRounding.AwayFromZero))
      : null;

    return new ChildOrderUpsertDto(
      parent.Id,
      parent.DealId,
      ownerWallet,
      contraWallet,
      dto.Status ?? parent.EscrowStatus,
      childFilled,
      dto.FillNonce,
      dto.FillPda
    );
  }


  private static void TryReleaseParentIfFullyFilled(EscrowOrderEntity entity)
  {
    var totalHuman = NormalizeAmount(entity.Amount, 1_000_000m);
    if (totalHuman is null) return;

    if (entity.FilledQuantity >= totalHuman.Value)
    {
      entity.EscrowStatus = EscrowStatus.Released;
      if (entity.ClosedAtUtc is null)
        entity.ClosedAtUtc = DateTime.UtcNow;
    }
  }

  private static decimal? NormalizeAmount(ulong? atomic, decimal multiplier)
  {
    return atomic.HasValue ? (decimal)atomic.Value / multiplier : null;
  }
}