using App.Strategy;
using Domain.Interfaces.Hooks.Parsing;
using Domain.Models.Api.Hooks.Solana;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        if (payload?.Data == null || payload.Data.Count == 0)
        {
            log.LogWarning("Received payload with no data: {RawPayload}", raw);
            return BadRequest("Payload data is empty.");
        }
        
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