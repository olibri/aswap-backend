using Domain.Models.Api.PaymentMethod;

namespace Domain.Interfaces.Services.PaymentMethod;

public interface IPaymentCatalog
{
  IReadOnlyList<PaymentDto> All { get; }
  IReadOnlyList<PaymentDto> Search(string q);
  Task ReloadAsync(CancellationToken ct = default);
}