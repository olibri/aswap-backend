namespace Domain.Models.Api.CoinPrice;

public sealed record PriceSnapshotUpsertDto(
  string TokenMint,
  string Quote,
  decimal Price,
  DateTime MinuteBucketUtc,
  DateTime CollectedAtUtc
);