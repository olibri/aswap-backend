using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Aswap_back.Configuration;

public sealed class EnumNamesInDescriptionParameterFilter : IParameterFilter
{
  public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
  {
    var t = context.PropertyInfo?.PropertyType ?? context.ParameterInfo?.ParameterType;
    if (t is null) return;

    var u = Nullable.GetUnderlyingType(t) ?? t;
    if (!u.IsEnum) return;

    var names = Enum.GetNames(u);
    var values = Enum.GetValues(u).Cast<object>().Select(Convert.ToInt64);

    var map = string.Join(", ", values.Zip(names, (v, n) => $"{v}={n}"));

    var prefix = string.IsNullOrWhiteSpace(parameter.Description) ? "" : parameter.Description.Trim() + " ";
    parameter.Description = $"{prefix}(Allowed: {map})";
  }
}