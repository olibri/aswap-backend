using Domain.Enums;
using Domain.Models.Enums;

namespace Domain.Models.Api.QuerySpecs;

public sealed record OffersQuery
(
    int Page = 1,
    int Size = 25,
    OfferSortField SortBy = OfferSortField.CreatedAt,
    SortDir Dir = SortDir.Desc,
    EscrowStatus? Status = null,
    OrderSide? Side = null,
    string? Fiat = null,
    string? TokenMint = null
);