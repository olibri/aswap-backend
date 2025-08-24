using Domain.Interfaces.Services.CoinService;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Services.CoinPrice.Workers;

public sealed class TokenBootstrapHostedService(IPriceIngestFacade facade, ILogger<TokenBootstrapHostedService> log)
  : IHostedService
{
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    try
    {
      await facade.BootstrapTokensAsync(cancellationToken);
      log.LogInformation("Token bootstrap completed");
    }
    catch (Exception ex)
    {
      log.LogError(ex, "Token bootstrap failed");
    }
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}