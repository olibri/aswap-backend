using Domain.Models.Api.PaymentMethod;

namespace Domain.Interfaces.Services.PaymentMethod;

public interface ICurrencyCatalog
{
  IReadOnlyList<CurrencyDto> All { get; }

  IReadOnlyList<CurrencyDto> Search(string query);
}