using Domain.Models;
using Domain.Models.Api.Hooks.Webhooks;

namespace Domain.Interfaces.Hooks;

public interface IWebhookProcessor
{
    Task<Result<string>> ProcessWebhook(WebhookRequest request);
}