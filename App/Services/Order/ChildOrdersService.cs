using Domain.Enums;
using Domain.Interfaces.Services.Order;
using Domain.Models.Api.Order;
using Domain.Models.DB;
using Domain.Models.Dtos;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Order;

public sealed class ChildOffersService(IDbContextFactory<P2PDbContext> dbFactory) : IChildOffersService
{
  public async Task<ChildOrderDto> UpsertAsync(ChildOrderUpsertDto dto, CancellationToken ct = default)
  {
    Validate(dto);
    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var parent = await db.EscrowOrders
      .AsNoTracking()
      .Where(x => x.Id == dto.ParentOrderId)
      .Select(x => new { x.Id, x.DealId, x.IsPartial })
      .SingleOrDefaultAsync(ct);

    if (parent is null)
      throw new InvalidOperationException($"Parent order '{dto.ParentOrderId}' not found.");


    if (parent.DealId != dto.DealId)
      throw new InvalidOperationException($"DealId mismatch: parent={parent.DealId}, dto={dto.DealId}.");

    ChildOrderEntity? entity = null;

    if (dto.EscrowStatus is EscrowStatus.SignedByContraAgentSide or EscrowStatus.SignedByOwnerSide)
      entity = await db.Set<ChildOrderEntity>()
        .FirstOrDefaultAsync(x =>
          x.ParentOrderId == dto.ParentOrderId, ct);

    var now = DateTime.UtcNow;

    if (entity is null)
    {
      entity = new ChildOrderEntity
      {
        Id = Guid.NewGuid(),
        ParentOrderId = dto.ParentOrderId,
        DealId = dto.DealId,
        OrderOwnerWallet = dto.OrderOwnerWallet,
        ContraAgentWallet = dto.ContraAgentWallet,
        EscrowStatus = dto.EscrowStatus,
        CreatedAtUtc = now,
        FilledAmount = dto.FilledAmount,
        FillNonce = dto.FillNonce,
        FillPda = dto.FillPda
      };

      db.Add(entity);
    }
    else
    {
      entity.EscrowStatus = dto.EscrowStatus;

      entity.FilledAmount = dto.FilledAmount;
      entity.FillPda = dto.FillPda;
      entity.FillNonce = dto.FillNonce;

      if (dto.EscrowStatus == EscrowStatus.Released) entity.ClosedAtUtc = now;
    }

    await db.SaveChangesAsync(ct);
    return ToDto(entity);
  }

  public async Task<IReadOnlyList<ChildOrderDto>> GetByParentAsync(long dealId, CancellationToken ct = default)
  {
    if (dealId < 0) throw new ArgumentOutOfRangeException(nameof(dealId));
    var deal = (ulong)dealId;

    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var items = await db.Set<ChildOrderEntity>()
      .AsNoTracking()
      .Where(x => x.DealId == deal)
      .OrderByDescending(x => x.CreatedAtUtc)
      .Select(x => new ChildOrderDto(
        x.Id,
        x.ParentOrderId,
        x.DealId,
        x.OrderOwnerWallet,
        x.ContraAgentWallet,
        x.EscrowStatus,
        x.CreatedAtUtc,
        x.ClosedAtUtc,
        x.FilledAmount,
        x.FillNonce,
        x.FillPda))
      .ToListAsync(ct);

    return items;
  }


  private static void Validate(ChildOrderUpsertDto dto)
  {
    if (dto.ParentOrderId == Guid.Empty)
      throw new ArgumentException("ParentOrderId is required.", nameof(dto));
    if (string.IsNullOrWhiteSpace(dto.OrderOwnerWallet))
      throw new ArgumentException("OrderOwnerWallet is required.", nameof(dto));
    if (string.IsNullOrWhiteSpace(dto.ContraAgentWallet))
      throw new ArgumentException("ContraAgentWallet is required.", nameof(dto));
    if (dto.OrderOwnerWallet == dto.ContraAgentWallet)
      throw new ArgumentException("OrderOwnerWallet and ContraAgentWallet must differ.", nameof(dto));
  }

  private static ChildOrderDto ToDto(ChildOrderEntity e)
  {
    return new ChildOrderDto(
      e.Id,
      e.ParentOrderId,
      e.DealId,
      e.OrderOwnerWallet,
      e.ContraAgentWallet,
      e.EscrowStatus,
      e.CreatedAtUtc,
      e.ClosedAtUtc,
      e.FilledAmount,
      e.FillNonce,
      e.FillPda
    );
  }
}