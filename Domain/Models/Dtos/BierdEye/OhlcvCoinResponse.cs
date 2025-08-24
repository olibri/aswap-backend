namespace Domain.Models.Dtos.BierdEye;

public class OhlcvCoinResponse
{
  public bool Success { get; set; }
  public OhlcvCoinData Data { get; set; } = null!;
}