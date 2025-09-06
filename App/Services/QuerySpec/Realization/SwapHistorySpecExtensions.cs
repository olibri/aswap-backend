using App.Services.QuerySpec.Realization.Helpers;
using Domain.Enums;
using Domain.Models.Api.Swap;
using Domain.Models.DB;

namespace App.Services.QuerySpec.Realization;

public static class SwapHistorySpecExtensions
{
  public static QuerySpec<AccountSwapHistoryEntity> BuildSpec(this SwapHistoryQuery q)
  {
    var spec = new QuerySpec<AccountSwapHistoryEntity>();

    spec.Where(new EqualFilter<AccountSwapHistoryEntity, string>(e => e.UserWallet, q.UserWallet));

    if (q.From is not null && q.To is not null)
      spec.Where(new DateRangeFilter<AccountSwapHistoryEntity>(e => e.CreatedAtUtc, q.From.Value, q.To.Value));
    else if (q.From is not null)
      spec.Where(new GreaterOrEqualFilter<AccountSwapHistoryEntity, DateTime>(e => e.CreatedAtUtc, q.From.Value));
    else if (q.To is not null)
      spec.Where(new EqualFilter<AccountSwapHistoryEntity, DateTime>(e => e.CreatedAtUtc, q.To.Value));

    var desc = q.Dir == SortDir.Desc;
    spec.OrderBy(q.SortBy switch
    {
      SwapHistorySortField.CryptoFrom => new SortRule<AccountSwapHistoryEntity>(e => e.CryptoFrom, desc),
      SwapHistorySortField.CryptoTo => new SortRule<AccountSwapHistoryEntity>(e => e.CryptoTo, desc),
      _ => new SortRule<AccountSwapHistoryEntity>(e => e.CreatedAtUtc, desc)
    });

    spec.Page(q.Page, q.Size);
    return spec;
  }
}