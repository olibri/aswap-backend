namespace Domain.Models.Api.QuerySpecs;

public sealed record PagedResult<T>(
  IReadOnlyList<T> Data,
  int Page, int Size, int Total);