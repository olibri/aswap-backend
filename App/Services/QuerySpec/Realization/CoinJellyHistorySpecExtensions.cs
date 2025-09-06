using App.Services.QuerySpec.Realization.Helpers;
using Domain.Enums;
using Domain.Models.Api.CoinJelly;
using Domain.Models.DB;
using System;

namespace App.Services.QuerySpec.Realization;

public static class CoinJellyHistorySpecExtensions
{
  public static QuerySpec<CoinJellyAccountHistoryEntity> BuildSpec(this CoinJellyHistoryQueryAsync q)
  {
    var spec = new QuerySpec<CoinJellyAccountHistoryEntity>();

    if (!string.IsNullOrWhiteSpace(q.UserWallet))
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, string>(e => e.UserWallet, q.UserWallet!));

    if (q.Status is not null)
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, Domain.Enums.CoinJellyStatus>(e => e.Status, q.Status.Value));

    if (!string.IsNullOrWhiteSpace(q.CryptoSend))
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, string>(e => e.CryptoSend, q.CryptoSend!));

    if (!string.IsNullOrWhiteSpace(q.CryptoGet))
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, string>(e => e.CryptoGet, q.CryptoGet!));

    if (!string.IsNullOrWhiteSpace(q.CryptoGetChain))
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, string>(e => e.CryptoGetChain, q.CryptoGetChain!));

    if (q.From is not null && q.To is not null)
      spec.Where(new DateRangeFilter<CoinJellyAccountHistoryEntity>(e => e.CreatedAtUtc, q.From.Value, q.To.Value));
    else if (q.From is not null)
      spec.Where(new GreaterOrEqualFilter<CoinJellyAccountHistoryEntity, DateTime>(e => e.CreatedAtUtc, q.From.Value));
    else if (q.To is not null)
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, DateTime>(e => e.CreatedAtUtc, q.To.Value));

    if (q.AmountSendFrom is not null)
      spec.Where(new GreaterOrEqualFilter<CoinJellyAccountHistoryEntity, decimal>(e => e.AmountSend, q.AmountSendFrom.Value));
    if (q.AmountSendTo is not null)
      spec.Where(new EqualFilter<CoinJellyAccountHistoryEntity, decimal>(e => e.AmountSend, q.AmountSendTo.Value));

    var desc = q.Dir == SortDir.Desc;
    spec.OrderBy(q.SortBy switch
    {
      JellySortField.AmountSend => new SortRule<CoinJellyAccountHistoryEntity>(e => e.AmountSend, desc),
      JellySortField.AmountGet => new SortRule<CoinJellyAccountHistoryEntity>(e => e.AmountGet, desc),
      JellySortField.Fee => new SortRule<CoinJellyAccountHistoryEntity>(e => e.FeeAtomic, desc),
      _ => new SortRule<CoinJellyAccountHistoryEntity>(e => e.CreatedAtUtc, desc),
    });

    spec.Page(q.Page, q.Size);
    return spec;
  }
}