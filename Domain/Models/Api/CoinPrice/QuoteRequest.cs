using Domain.Enums;

namespace Domain.Models.Api.CoinPrice;

public sealed record QuoteRequest(
  string InputMint,
  string OutputMint,
  ulong Amount,
  SwapMode SwapMode = SwapMode.ExactIn,
  ushort SlippageBps = 50
);