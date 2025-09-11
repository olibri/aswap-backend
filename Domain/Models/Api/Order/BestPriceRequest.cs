using Domain.Models.Enums;

namespace Domain.Models.Api.Order;

public sealed class BestPriceRequest
{
  public OrderSide Side { get; set; }
  public string FiatCode { get; set; }

  public string TokenMint { get; set; }
};