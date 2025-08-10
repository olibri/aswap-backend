namespace Domain.Models.Api.Metrics;

public sealed record DashboardQuery(DateTime? From = null, DateTime? To = null)
{
  public (DateTime From, DateTime To) Normalize()
  {
    var to = (To ?? DateTime.UtcNow).Date;
    var from = (From ?? to.AddDays(-7)).Date;   
    if (from > to) (from, to) = (to, from);
    return (from, to);
  }
}