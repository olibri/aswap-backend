using Domain.Models.Api.Order;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Dtos;

namespace Domain.Interfaces.Database.Queries;

public interface IMarketDbQueries
{
    Task<EscrowOrderDto> GetNewOfferAsync(ulong dealId);

    Task<PagedResult<EscrowOrderDto>> GetAllNewOffersAsync(OffersQuery q, CancellationToken ct = default);
    Task<EscrowOrderDto[]> GetAllAdminOffersAsync();

    public Task<PagedResult<EscrowOrderDto>> GetAllUsersOffersAsync(string userId, UserOffersQuery q);

    Task<EscrowOrderDto?> CheckOrderStatusAsync(ulong orderId);
}