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
  
  [property:
    SwaggerSchema(Description =
      "UniversalOrderStatus: 0=Created, 1=Active, 3=SignedByOneParty, 4=BothSigned, 5=Completed, 6=Cancelled, 7=AdminResolving")]
  UniversalOrderStatus? Status = null,
  
  [property: SwaggerSchema(Description = "OrderSide: 0=Sell, 1=Buy")]
  OrderSide? Side = null,
  
  string? Fiat = null,
  string? TokenMint = null,
  decimal? PriceFrom = null,
  string[] PaymentMethod = null
);