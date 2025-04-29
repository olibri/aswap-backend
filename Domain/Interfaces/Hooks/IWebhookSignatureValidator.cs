using Domain.Models.Api.Hooks;

namespace Domain.Interfaces.Hooks;

public interface IWebhookSignatureValidator
{
    bool VerifySignature(WebhookRequest request);
}