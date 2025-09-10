namespace Domain.Models.Api.QuerySpecs;

public record NotificationQuery : PageSpec
{
  public bool? IsRead { get; set; }
  public DateTime? FromDate { get; set; }
  public DateTime? ToDate { get; set; }
}