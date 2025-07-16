using System.Text.Json;
using Domain.Interfaces;

namespace Domain.Models;

public class SystemTextJsonSerializer : IJsonSerializer
{
  private static readonly JsonSerializerOptions Opt = new(JsonSerializerDefaults.Web);
  public string ToJson<T>(T o) => JsonSerializer.Serialize(o, Opt);
  public T FromJson<T>(string j) => JsonSerializer.Deserialize<T>(j, Opt)!;
}