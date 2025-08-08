using Domain.Enums;
using Domain.Models.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace Domain.Models.Api.QuerySpecs;

public sealed record OffersQuery(
    int Page = 1,
    int Size = 25,

    [property: SwaggerSchema(Description = "OfferSortField: 0=CreatedAt, 1=Price, 2=Amount")]
    OfferSortField SortBy = OfferSortField.CreatedAt,

    [property: SwaggerSchema(Description = "SortDir: 0=Asc, 1=Desc")]
    SortDir Dir = SortDir.Desc,

    [property: SwaggerSchema(Description = "EscrowStatus: 0=PendingOnChain, 1=OnChain, 2=PartiallyOnChain, 3=Signed, 4=SignedByOneSide, 5=Released, 6=Cancelled, 7=AdminResolving")]
    EscrowStatus? Status = null,

    [property: SwaggerSchema(Description = "OrderSide: 0=Sell, 1=Buy")]
    OrderSide? Side = null,

    string? Fiat = null,
    string? TokenMint = null
);
