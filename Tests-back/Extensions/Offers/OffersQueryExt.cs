using Domain.Enums;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.Enums;

namespace Tests_back.Extensions.Offers;

public static class OffersQueryExt
{
  public static OffersQuery With(
    this OffersQuery q,
    int? page = null, int? size = null,
    OfferSortField? sort = null, SortDir? dir = null,
    EscrowStatus? status = null, OrderSide? side = null,
    string? fiat = null, string? mint = null)
    => q with
    {
      Page = page ?? q.Page,
      Size = size ?? q.Size,
      SortBy = sort ?? q.SortBy,
      Dir = dir ?? q.Dir,
      Status = status ?? q.Status,
      Side = side ?? q.Side,
      Fiat = fiat ?? q.Fiat,
      TokenMint = mint ?? q.TokenMint
    };
}