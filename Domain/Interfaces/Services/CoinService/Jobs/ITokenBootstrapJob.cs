namespace Domain.Interfaces.Services.CoinService.Jobs;

public interface ITokenBootstrapJob
{
  // 1) На старті: якщо Tokens порожня — тягнемо з Jup і пишемо в БД
  Task RunOnceAsync(CancellationToken ct);
}