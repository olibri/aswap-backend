using App.Mapper;
using Domain.Enums;
using Domain.Interfaces.Database.Command;
using Domain.Interfaces.Services.Order;
using Domain.Models.Api.Order;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Domain.Models.Events;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Domain.Interfaces.Services.Notification;
using Domain.Models.Enums;
using System.Text.Json;

namespace App.Db;

public class MarketDbCommand(
  P2PDbContext dbContext,
  IChildOffersService childOffers,
  INotificationService notificationService) : IMarketDbCommand
{
  public async Task CreateOfferAsync(UniversalOrderCreated offer)
  {
    var mappedEntity = EscrowOrderMapper.ToEntity(offer);
    mappedEntity.DomainEvents.Add(new OfferCreated(
      Guid.NewGuid(), mappedEntity.Id, mappedEntity.OrderId,
      mappedEntity.CreatorWallet, mappedEntity.OfferSide, EventType.OfferCreated));

    await dbContext.EscrowOrders.AddAsync(mappedEntity);
    await dbContext.SaveChangesAsync();
  }

  public async Task<ulong> CreateBuyerOfferAsync(UpsertOrderDto dto)
  {
    var exists = await dbContext.EscrowOrders
      .AsNoTracking()
      .AnyAsync(x => x.OrderId == dto.OrderId);
    if (exists)
      throw new InvalidOperationException($"Order with OrderId={dto.OrderId} already exists.");

    await using var tx = await dbContext.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);

    var entity = EscrowOrderMapper.ToEntity(dto);
    entity.OfferSide = OrderSide.Buy;
    entity.CreatedAtUtc = DateTime.UtcNow;
    entity.Id = entity.Id == Guid.Empty ? Guid.NewGuid() : entity.Id;
    entity.DomainEvents.Add(new OfferCreated(
      Guid.NewGuid(), entity.Id, entity.OrderId, entity.CreatorWallet, OrderSide.Buy, EventType.OfferCreated));

    await dbContext.EscrowOrders.AddAsync(entity);
    await dbContext.SaveChangesAsync();

    var desiredIds = dto.PaymentMethodIds.Distinct().ToArray();
    var validIds = await dbContext.PaymentMethods
      .Where(pm => desiredIds.Contains(pm.Id))
      .Select(pm => pm.Id)
      .ToListAsync();

    if (validIds.Count != desiredIds.Length)
      throw new ValidationException("Some payment methods don't exist.");

    var links = validIds.Select(id => new EscrowOrderPaymentMethodEntity
    {
      OrderId = entity.Id,
      MethodId = id
    });

    await dbContext.AddRangeAsync(links);
    await dbContext.SaveChangesAsync();

    await tx.CommitAsync();
    return entity.OrderId;
  }

  // Simplified: no internal status machine, no timing / notification logic.
  public async Task<EscrowOrderDto> UpdateCurrentOfferAsync(UpsertOrderDto dto, CancellationToken ct = default)
  {
    if (dto is null) throw new ArgumentNullException(nameof(dto));

    ScaleFilledQuantity(dto);
    var order = await LoadOrderAsync(dto.OrderId, ct);
    var previousStatus = order.Status;

    if (dto.IsPartial == true)
    {
      await ApplyPartialAsync(order, dto, ct);
    }
    else
    {
      ApplyNonPartial(order, dto);
    }

    await dbContext.SaveChangesAsync(ct);
    await NotifyStatusChange(order, previousStatus, order.Status, ct);

    return EscrowOrderDto.FromEntity(order);
  }

  private static EscrowOrderPatchResult ApplyNonPartial(EscrowOrderEntity order, UpsertOrderDto dto)
    => EscrowOrderPatcher.ApplyUpsert(order, dto);

  private async Task NotifyStatusChange(
    EscrowOrderEntity order,
    UniversalOrderStatus oldStatus,
    UniversalOrderStatus newStatus,
    CancellationToken ct = default)
  {
    if (oldStatus == newStatus) return;

    var wallets = new[]
      {
        order.CreatorWallet,
        order.AcceptorWallet
      }
      .Where(w => !string.IsNullOrWhiteSpace(w))
      .Distinct(StringComparer.Ordinal)
      .ToArray();

    if (wallets.Length == 0) return;

    var statusMessage = GetStatusMessage(newStatus);
    var metadata = JsonSerializer.Serialize(new
    {
      OrderId = order.OrderId,
      OldStatus = oldStatus,
      NewStatus = newStatus
    });

    foreach (var wallet in wallets)
    {
      await notificationService.CreateNotificationAsync(
        wallet!,
        "Order Status Changed",
        $"Order #{order.OrderId} status: {statusMessage}",
        NotificationType.OrderStatusChanged,
        relatedEntityId: order.OrderId.ToString(),
        metadata: metadata,
        ct: ct);
    }
  }

  private static string GetStatusMessage(UniversalOrderStatus status) =>
    status switch
    {
      UniversalOrderStatus.BothSigned => "Signed by both parties",
      UniversalOrderStatus.SignedByOneParty => "Signed by one party",
      UniversalOrderStatus.Completed => "Completed",
      UniversalOrderStatus.Cancelled => "Cancelled",
      UniversalOrderStatus.AdminResolving => "Admin resolving",
      UniversalOrderStatus.Active => "Active",
      UniversalOrderStatus.Created => "Created",
      _ => status.ToString()
    };

  private static void ScaleFilledQuantity(UpsertOrderDto dto)
  {
    if (dto.FilledQuantity.HasValue)
      dto.FilledQuantity *= 1_000_000m;
  }

  private async Task<EscrowOrderEntity> LoadOrderAsync(ulong orderId, CancellationToken ct)
  {
    var entity = await dbContext.EscrowOrders
      .Include(x => x.PaymentMethods)
      .FirstOrDefaultAsync(x => x.OrderId == orderId, ct);

    return entity ?? throw new InvalidOperationException($"EscrowOrderEntity with OrderId {orderId} not found.");
  }

  private async Task ApplyPartialAsync(EscrowOrderEntity order, UpsertOrderDto dto, CancellationToken ct)
  {
    order.IsPartial = true;

    if (!string.IsNullOrWhiteSpace(dto.CreatorWallet))
      order.CreatorWallet = dto.CreatorWallet;
    if (!string.IsNullOrWhiteSpace(dto.AcceptorWallet))
      order.AcceptorWallet = dto.AcceptorWallet;
    if (dto.Status.HasValue)
      order.Status = dto.Status.Value;

    if (dto.FilledQuantity.HasValue)
      order.FilledQuantity += dto.FilledQuantity.Value;

    
    // Child ticket
    if (!string.IsNullOrWhiteSpace(order.CreatorWallet) &&
        !string.IsNullOrWhiteSpace(order.AcceptorWallet) &&
        dto.FilledQuantity.HasValue)
    {
      var childDto = BuildChildDto(order, dto);
      await childOffers.UpsertAsync(childDto, ct);
    }
  }

  private static ChildOrderUpsertDto BuildChildDto(EscrowOrderEntity order, UpsertOrderDto source)
  {
    var owner = order.CreatorWallet
      ?? throw new InvalidOperationException("CreatorWallet required for child creation.");
    var contra = source.AcceptorWallet ?? order.AcceptorWallet
      ?? throw new InvalidOperationException("AcceptorWallet required for child creation.");

    return new ChildOrderUpsertDto(
      order.Id,
      source.TicketId ?? order.OrderId,
      owner,
      contra,
      source.Status ?? order.Status,
      source.FilledQuantity,
      source.TicketPda
    );
  }
}