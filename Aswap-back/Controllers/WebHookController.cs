using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Domain.Models.Api.Hooks;
using Domain.Interfaces.Hooks.Parsing;

namespace Aswap_back.Controllers;

//TODO: Add signature for getting data
[ApiController]
public class WebHookController(IAnchorEventParser parser)
    : Controller
{
    [HttpPost]
    [Route("webhook")]
    public async Task<IActionResult> QuickNodeCallback()
    {
        var raw = await new StreamReader(Request.Body).ReadToEndAsync();

        var payload = JsonSerializer.Deserialize<SolanaWebhookPayload>(raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var events = parser.Parse(payload!.Data[0].Logs);

        foreach (var ev in events)
            Console.WriteLine(ev);

        return Ok();
    }
}