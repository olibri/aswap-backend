using Domain.Interfaces.Hooks;

namespace Domain.Models.Api.Hooks.Webhooks;

public class WebhookRequest
{
    public WebhookHeaders Headers { get; set; }
    public string Payload { get; set; }
    public IWebhookSignatureValidator SignatureValidator { get; set; }
    public string StreamId { get; set; }
}