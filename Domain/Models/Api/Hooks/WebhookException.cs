using Microsoft.AspNetCore.Http;

namespace Domain.Models.Api.Hooks;

public class WebhookException(string message, int statusCode = StatusCodes.Status400BadRequest)
    : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}