using Domain.Interfaces.Hooks;
using Domain.Models.Api.Hooks;
using Microsoft.AspNetCore.Mvc;

namespace Aswap_back.Controllers;

[ApiController]
public class WebHookController(IWebhookProcessor webhookProcessor)
    : ControllerBase
{
    [HttpPost]
    [Route("{streamId}/webhook")]
    public async Task<IActionResult> QuickNodeCallback(string streamId)
    {
        var request = new WebhookRequest
        {
            Headers = HttpContext.Items["WebhookHeaders"] as WebhookHeaders,
            Payload = HttpContext.Items["WebhookPayload"] as string,
            SignatureValidator = HttpContext.Items["WebhookSignatureValidator"] as IWebhookSignatureValidator,
            StreamId = streamId
        };

        var result = await webhookProcessor.ProcessWebhook(request);
        return Ok(result);
    }
}