using App.Services.QuerySpec.Realization.Helpers;
using Domain.Enums;
using Domain.Models.Api.QuerySpecs;
using Domain.Models.DB;
using Domain.Models.Enums;

namespace App.Services.QuerySpec.Realization;

public static class OfferSpecExtensions
{
  public static QuerySpec<EscrowOrderEntity> BuildSpec(this OffersQuery q)
  {
    var spec = new QuerySpec<EscrowOrderEntity>();

    if (q.Status is not null)
      spec.Where(new EqualFilter<EscrowOrderEntity, UniversalOrderStatus>(o => o.Status, q.Status.Value));

    if (q.Side is not null)
      spec.Where(new EqualFilter<EscrowOrderEntity, OrderSide>(o => o.OfferSide, q.Side.Value));

    if (!string.IsNullOrWhiteSpace(q.Fiat))
      spec.Where(new EqualFilter<EscrowOrderEntity, string>(o => o.FiatCode, q.Fiat!));

    if (!string.IsNullOrWhiteSpace(q.TokenMint))
      spec.Where(new EqualFilter<EscrowOrderEntity, string>(o => o.TokenMint, q.TokenMint!));

    if (q.PriceFrom is not null)
      spec.Where(new GreaterOrEqualFilter<EscrowOrderEntity, decimal>(o => o.Price, q.PriceFrom.Value));

    if (q.PaymentMethod is { Length: > 0 })
      spec.Where(new AnyInCollectionFilter<EscrowOrderEntity, EscrowOrderPaymentMethodEntity, string>(
        o => o.PaymentMethods,
        pm => pm.Method.Code,
        q.PaymentMethod
      ));

    var desc = q.Dir == SortDir.Desc;
    spec.OrderBy(q.SortBy switch
    {
      OfferSortField.Price => new SortRule<EscrowOrderEntity>(o => o.Price, desc),
      OfferSortField.Amount => new SortRule<EscrowOrderEntity>(o => o.Amount, desc),
      _ => new SortRule<EscrowOrderEntity>(o => o.CreatedAtUtc, desc)
    });

    spec.Page(q.Page, q.Size);
    return spec;
  }
}
