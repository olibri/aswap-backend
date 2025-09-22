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
      .Select(x => new { x.Id, x.OrderId, x.IsPartial })
      .SingleOrDefaultAsync(ct);

    if (parent is null)
      throw new InvalidOperationException($"Parent order '{dto.ParentOrderId}' not found.");

    if (parent.OrderId != dto.TicketId)
      throw new InvalidOperationException($"TicketId mismatch: parent={parent.OrderId}, dto={dto.TicketId}.");

    UniversalTicketEntity? entity = null;

    if (dto.Status is UniversalOrderStatus.SignedByOneParty or UniversalOrderStatus.BothSigned)
      entity = await db.Set<UniversalTicketEntity>()
        .FirstOrDefaultAsync(x =>
          x.ParentOrderId == dto.ParentOrderId, ct);

    var now = DateTime.UtcNow;

    if (entity is null)
    {
      entity = new UniversalTicketEntity
      {
        Id = Guid.NewGuid(),
        ParentOrderId = dto.ParentOrderId,
        TicketId = dto.TicketId,
        OrderOwnerWallet = dto.OrderOwnerWallet,
        ContraAgentWallet = dto.ContraAgentWallet,
        Status = dto.Status,
        CreatedAtUtc = now,
        Amount = dto.Amount,
        TicketPda = dto.TicketPda
      };

      db.Add(entity);
    }
    else
    {
      entity.Status = dto.Status;
      entity.Amount = dto.Amount;
      entity.TicketPda = dto.TicketPda;
      entity.UpdatedAt = now;

      if (dto.Status == UniversalOrderStatus.Completed) 
        entity.ClosedAtUtc = now;
    }

    await db.SaveChangesAsync(ct);
    return ChildOrderDto.FromEntity(entity);
  }

  public async Task<IReadOnlyList<ChildOrderDto>> GetByParentAsync(long ticketId, CancellationToken ct = default)
  {
    if (ticketId < 0) throw new ArgumentOutOfRangeException(nameof(ticketId));
    var ticket = (ulong)ticketId;

    await using var db = await dbFactory.CreateDbContextAsync(ct);

    var items = await db.Set<UniversalTicketEntity>()
      .AsNoTracking()
      .Where(x => x.TicketId == ticket)
      .OrderByDescending(x => x.CreatedAtUtc)
      .ToListAsync(ct);

    return items.Select(ChildOrderDto.FromEntity).ToList();
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
}