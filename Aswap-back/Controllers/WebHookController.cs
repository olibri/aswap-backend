using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Domain.Models.Api.Hooks;
using Domain.Interfaces.Hooks.Parsing;
using App.Strategy;

namespace Aswap_back.Controllers;

//TODO: Add signature for getting data
[ApiController]
public class WebHookController(
    IAnchorEventParser parser,
    AnchorEventDispatcher dispatcher,
    ILogger<WebHookController> log)
    : Controller
{
    [HttpPost]
    [Route("webhook")]
    public async Task<IActionResult> QuickNodeCallback(CancellationToken ct)
    {
        var raw = await new StreamReader(Request.Body).ReadToEndAsync(ct);

        var payload = JsonSerializer.Deserialize<SolanaWebhookPayload>(raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var events = parser.Parse(payload!.Data[0].Logs);

        foreach (var ev in events)
            try
            {
                await dispatcher.DispatchAsync(ev, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to handle {Event}", ev);
            }

        return Ok();
    }
}