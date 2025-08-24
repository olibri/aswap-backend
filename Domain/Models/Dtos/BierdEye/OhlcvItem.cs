namespace Domain.Models.Dtos.BierdEye;

public class OhlcvItem
{
  public decimal O { get; set; }   // open
  public decimal H { get; set; }   // high
  public decimal L { get; set; }   // low
  public decimal C { get; set; }   // close
  public decimal V { get; set; }   // volume

  public long UnixTime { get; set; }
  public string Address { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Currency { get; set; } = string.Empty;
}