using Microsoft.AspNetCore.Http;

namespace Domain.Models.Api.Hooks.Webhooks;

public class WebhookException(string message, int statusCode = StatusCodes.Status400BadRequest)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}