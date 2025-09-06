namespace Domain.Interfaces.Services.PaymentMethod;

public interface IPopularityCounter
{
  void Hit(short[] methodIds, string region);
  Task<IReadOnlyList<short>> Top(string region, int top, CancellationToken ct);
}