namespace Domain.Interfaces;

public interface IJsonSerializer
{
  string ToJson<T>(T obj);
  T FromJson<T>(string json);
}