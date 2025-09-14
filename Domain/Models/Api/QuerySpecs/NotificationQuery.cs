using Swashbuckle.AspNetCore.Annotations;

namespace Domain.Models.Api.QuerySpecs;

public sealed record NotificationQuery(
  int Page = 1,
  int Size = 20,

  [property: SwaggerSchema(Description = "Filter by read status: true=read, false=unread, null=all")]
  bool? IsRead = null,

  [property: SwaggerSchema(Description = "Filter notifications from this date")]
  DateTime? FromDate = null,

  [property: SwaggerSchema(Description = "Filter notifications until this date")]
  DateTime? ToDate = null
)
{
  public int Skip => (Page - 1) * Size;
  public int Take => Size;
}