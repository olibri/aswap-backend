namespace App.Utils;

public static class DateTimeExtensions
{
  /// <summary>
  /// Trims seconds and milliseconds, leaving only yyyy-MM-dd HH:mm:00.і
  /// </summary>
  public static DateTime TrimToMinute(this DateTime dtUtc)
    => new(
      dtUtc.Year, dtUtc.Month, dtUtc.Day,
      dtUtc.Hour, dtUtc.Minute, 0, DateTimeKind.Utc);
}