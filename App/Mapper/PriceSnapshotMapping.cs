using Domain.Models.DB.CoinPrice;
using Domain.Models.Dtos;

namespace App.Mapper;

public static class PriceSnapshotMapping
{
  public static TokenDailyPriceResponse ToDailyResponse(
    string tokenMint,
    string quote,
    DateOnly dateUtc,
    IEnumerable<PriceSnapshotEntity> snapshotsForDay)
  {
    var ordered = snapshotsForDay
      .Where(s => s.TokenMint == tokenMint &&
                  DateOnly.FromDateTime(s.MinuteBucketUtc) == dateUtc)
      .OrderBy(s => s.MinuteBucketUtc)
      .ToList();

    var points = ordered
      .Select(s => new PricePointDto
      {
        Time = s.MinuteBucketUtc,
        Price = s.Price
      })
      .ToList();

    return new TokenDailyPriceResponse
    {
      TokenMint = tokenMint,
      DateUtc = dateUtc,
      Points = points,
      FirstCollectedUtc = ordered.FirstOrDefault()?.CollectedAtUtc,
      LastCollectedUtc = ordered.LastOrDefault()?.CollectedAtUtc
    };
  }
}