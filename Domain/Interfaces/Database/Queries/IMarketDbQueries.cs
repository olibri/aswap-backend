using Domain.Models.Api.QuerySpecs;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Database.Queries;

public interface IMarketDbQueries
{
    Task<EscrowOrderDto> GetNewOfferAsync(ulong dealId);

    Task<EscrowOrderDto[]> GetAllNewOffersAsync(OffersQuery q, CancellationToken ct = default);
    Task<EscrowOrderDto[]> GetAllAdminOffersAsync();

    Task<EscrowOrderDto[]> GetAllUsersOffersAsync(string userId);

    Task<EscrowOrderDto?> CheckOrderStatusAsync(ulong orderId);
}