namespace Domain.Models.Dtos.BierdEye;

public class OhlcvCoinData
{
  public bool IsScaledUiToken { get; set; }
  public List<OhlcvItem> Items { get; set; } = new();
}