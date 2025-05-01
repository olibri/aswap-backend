using Microsoft.AspNetCore.Http;

namespace Domain.Models.Api.Hooks.Webhooks;

public class WebhookHeaders
{
    public string Nonce { get; set; }
    public string Timestamp { get; set; }
    public string Signature { get; set; }

    public bool IsValid() =>
        !string.IsNullOrEmpty(Nonce) &&
        !string.IsNullOrEmpty(Timestamp) &&
        !string.IsNullOrEmpty(Signature);

    public static WebhookHeaders FromRequest(HttpRequest request)
    {
        return new WebhookHeaders
        {
            Nonce = request.Headers["x-qn-nonce"].ToString(),
            Timestamp = request.Headers["x-qn-timestamp"].ToString(),
            Signature = request.Headers["x-qn-signature"].ToString()
        };
    }
}