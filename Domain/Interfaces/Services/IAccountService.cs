namespace Domain.Interfaces.Services;

public interface IAccountService
{
  Task TouchAsync(string wallet, CancellationToken ct);
}