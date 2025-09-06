using App.Services.QuerySpec.Realization.Helpers;
using Domain.Enums;
using Domain.Models.Api.Order;
using Domain.Models.DB;
using Domain.Models.Enums;

namespace App.Services.QuerySpec.Realization;

public static class UserOffersSpecExtensions
{
  public static QuerySpec<EscrowOrderEntity> BuildSpec(this UserOffersQuery q)
  {
    var spec = new QuerySpec<EscrowOrderEntity>();

    if (q.Status is not null)
      spec.Where(new EqualFilter<EscrowOrderEntity, EscrowStatus>(e => e.EscrowStatus, q.Status.Value));

    if (!string.IsNullOrWhiteSpace(q.TokenMint))
      spec.Where(new EqualFilter<EscrowOrderEntity, string>(e => e.TokenMint!, q.TokenMint!));

    if (!string.IsNullOrWhiteSpace(q.FiatCode))
      spec.Where(new EqualFilter<EscrowOrderEntity, string>(e => e.FiatCode, q.FiatCode!));

    if (q.Side is not null)
      spec.Where(new EqualFilter<EscrowOrderEntity, OrderSide>(e => e.OfferSide, q.Side.Value));

    if (q.From is not null && q.To is not null)
      spec.Where(new DateRangeFilter<EscrowOrderEntity>(e => e.CreatedAtUtc, q.From.Value, q.To.Value));
    else if (q.From is not null)
      spec.Where(new GreaterOrEqualFilter<EscrowOrderEntity, DateTime>(e => e.CreatedAtUtc, q.From.Value));
    else if (q.To is not null)
      spec.Where(new DateRangeFilter<EscrowOrderEntity>(e => e.CreatedAtUtc, DateTime.MinValue, q.To.Value));
    
    var desc = q.Dir == SortDir.Desc;
    spec.OrderBy(new SortRule<EscrowOrderEntity>(e => e.Price, desc));

    spec.Page(q.Page, q.Size);
    return spec;
  }
}