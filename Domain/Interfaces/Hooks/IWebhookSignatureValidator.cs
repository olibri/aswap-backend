using Domain.Models.Api.Hooks.Webhooks;

namespace Domain.Interfaces.Hooks;

public interface IWebhookSignatureValidator
{
    bool VerifySignature(WebhookRequest request);
}