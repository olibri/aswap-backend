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
  public async Task CreateOfferAsync(UniversalOrderCreated offer)
  {
    try
    {
      var mappedEntity = EscrowOrderMapper.ToEntity(offer);

      mappedEntity.DomainEvents.Add(new OfferCreated(
        Guid.NewGuid(), mappedEntity.Id, mappedEntity.OrderId,
        mappedEntity.CreatorWallet, mappedEntity.OfferSide, EventType.OfferCreated));

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
      .AnyAsync(x => x.OrderId == dto.OrderId, CancellationToken.None);
    if (exists)
      throw new InvalidOperationException($"Order with OrderId={dto.OrderId} already exists.");

    await using var tx =
      await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, CancellationToken.None);

    var entity = EscrowOrderMapper.ToEntity(dto);
    entity.OfferSide = OrderSide.Buy;
    entity.CreatedAtUtc = DateTime.UtcNow;
    entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;

    entity.DomainEvents.Add(new OfferCreated(
      Guid.NewGuid(),
      entity.Id,
      entity.OrderId,
      entity.CreatorWallet,
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
      throw new ValidationException("Some payment methods don't exist.");

    var links = validIds.Select(id => new EscrowOrderPaymentMethodEntity
    {
      OrderId = entity.Id,
      MethodId = id
    });

    await dbContext.AddRangeAsync(links, CancellationToken.None);
    await dbContext.SaveChangesAsync(CancellationToken.None);

    await tx.CommitAsync(CancellationToken.None);

    return entity.OrderId;
  }

  public async Task UpdateCurrentOfferAsync(UpsertOrderDto upsertOrder)
  {
    //TODO: remove this delay, it's just for testing purposes
    await Task.Delay(4000);

    upsertOrder.FilledQuantity *= 1000000m;
    var entity = await dbContext.EscrowOrders
      .Include(x => x.PaymentMethods)
      .FirstOrDefaultAsync(x => x.OrderId == upsertOrder.OrderId);

    if (entity is null)
      throw new InvalidOperationException($"EscrowOrderEntity with OrderId {upsertOrder.OrderId} was not found.");

    var previousStatus = entity.Status;
    var now = DateTime.UtcNow;
    entity = TrackReleaseTime(entity, previousStatus, upsertOrder.Status, now);

    if (upsertOrder.IsPartial == true)
    {
      var childDto = BuildChildUpsertFrom(upsertOrder, entity);
      var childDtoUpdated = await childOffers.UpsertAsync(childDto, CancellationToken.None);
      if (childDtoUpdated.Status is UniversalOrderStatus.SignedByOneParty or UniversalOrderStatus.BothSigned)
      {
        await NotifyStatusChange(entity, previousStatus, entity.Status);
        return;
      }

      entity.OrderPda = upsertOrder.OrderPda ?? entity.OrderPda;
      entity.IsPartial = true;
      entity.Status = upsertOrder.Status ?? entity.Status;

      entity.CreatorWallet = upsertOrder.CreatorWallet ?? entity.CreatorWallet;
      entity.AcceptorWallet = upsertOrder.AcceptorWallet ?? entity.AcceptorWallet;

      if (upsertOrder.FilledQuantity.HasValue)
      {
        entity.FilledQuantity += upsertOrder.FilledQuantity.Value;
        //TryReleaseParentIfFullyFilled(entity);
      }

      await dbContext.SaveChangesAsync(CancellationToken.None);
      await NotifyStatusChange(entity, previousStatus, entity.Status);

      return;
    }

    EscrowOrderPatcher.ApplyUpsert(entity, upsertOrder);

    await dbContext.SaveChangesAsync();
    await NotifyStatusChange(entity, previousStatus, entity.Status);
  }

  private static EscrowOrderEntity TrackReleaseTime(EscrowOrderEntity entity, UniversalOrderStatus previousStatus,
    UniversalOrderStatus? newStatus, DateTime now)
  {
    if (newStatus == null) return entity;

    if (previousStatus != UniversalOrderStatus.SignedByOneParty &&
        previousStatus != UniversalOrderStatus.BothSigned &&
        (newStatus == UniversalOrderStatus.SignedByOneParty || newStatus == UniversalOrderStatus.BothSigned))
      entity.PaymentConfirmedAt = now;

    if ((previousStatus == UniversalOrderStatus.SignedByOneParty) &&
        newStatus == UniversalOrderStatus.BothSigned &&
        entity.PaymentConfirmedAt.HasValue)
    {
      entity.CryptoReleasedAt = now;
      entity.ReleaseTimeSeconds = (int)(now - entity.PaymentConfirmedAt.Value).TotalSeconds;
    }

    return entity;
  }


  private async Task NotifyStatusChange(EscrowOrderEntity order, UniversalOrderStatus oldStatus, UniversalOrderStatus newStatus)
  {
    if (oldStatus == newStatus) return;

    var usersToNotify = new List<string>();
    if (!string.IsNullOrEmpty(order.CreatorWallet) && order.CreatorWallet != "11111111111111111111111111111111")
      usersToNotify.Add(order.CreatorWallet);
    if (!string.IsNullOrEmpty(order.AcceptorWallet) && order.AcceptorWallet != "11111111111111111111111111111111")
      usersToNotify.Add(order.AcceptorWallet);

    var statusMessage = GetStatusMessage(newStatus);
    var metadata =
      JsonSerializer.Serialize(new { OrderId = order.OrderId, OldStatus = oldStatus, NewStatus = newStatus });

    foreach (var userWallet in usersToNotify)
      await notificationService.CreateNotificationAsync(
        userWallet,
        "Order Status Changed",
        $"Your order #{order.OrderId} status changed to {statusMessage}",
        NotificationType.OrderStatusChanged,
        order.OrderId.ToString(),
        metadata);
  }

  private static string GetStatusMessage(UniversalOrderStatus status)
  {
    return status switch
    {
      UniversalOrderStatus.BothSigned => "Signed by both parties",
      UniversalOrderStatus.SignedByOneParty => "Signed by one party",
      UniversalOrderStatus.Completed => "Trade completed successfully",
      UniversalOrderStatus.Cancelled => "Cancelled",
      UniversalOrderStatus.AdminResolving => "Under admin review",
      _ => status.ToString()
    };
  }

  private static ChildOrderUpsertDto BuildChildUpsertFrom(UpsertOrderDto dto, EscrowOrderEntity parent)
  {
    var ownerWallet = parent.CreatorWallet;
    var contraWallet = dto.AcceptorWallet;

    if (string.IsNullOrWhiteSpace(ownerWallet))
      throw new ArgumentException("Owner wallet is required for child upsert (derived from dto.CreatorWallet/dto.AcceptorWallet).",
        nameof(dto));
    if (string.IsNullOrWhiteSpace(contraWallet))
      throw new ArgumentException("Contra wallet is required for child upsert (derived from dto.CreatorWallet/dto.AcceptorWallet).",
        nameof(dto));

    decimal? childAmount = dto.FilledQuantity;

    return new ChildOrderUpsertDto(
      parent.Id,
      parent.OrderId,
      ownerWallet,
      contraWallet,
      dto.Status ?? parent.Status,
      childAmount,
      dto.TicketPda
    );
  }

  private static void TryReleaseParentIfFullyFilled(EscrowOrderEntity entity)
  {
    var totalHuman = NormalizeAmount(entity.Amount, 1_000_000m);
    if (totalHuman is null) return;

    if (entity.FilledQuantity >= totalHuman.Value)
    {
      entity.Status = UniversalOrderStatus.Completed;
      if (entity.ClosedAtUtc is null)
        entity.ClosedAtUtc = DateTime.UtcNow;
    }
  }

  private static decimal? NormalizeAmount(ulong? atomic, decimal multiplier)
  {
    return atomic.HasValue ? (decimal)atomic.Value / multiplier : null;
  }
}