using Domain.Interfaces.Hooks;
using Domain.Models.Api.Hooks.Webhooks;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace App.WebhookSettings;

public class WebhookSignatureValidator(string secretKey, ILogger<WebhookSignatureValidator> logger)
    : IWebhookSignatureValidator
{
    public bool VerifySignature(WebhookRequest request)
    {
        var signatureData = request.Headers.Nonce + request.Headers.Timestamp + request.Payload;

        var signatureBytes = Encoding.UTF8.GetBytes(signatureData);
        var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);

        using var hmac = new HMACSHA256(secretKeyBytes);
        var computedSignatureBytes = hmac.ComputeHash(signatureBytes);
        var computedSignature = BitConverter.ToString(computedSignatureBytes)
            .Replace("-", "")
            .ToLower();

        logger.LogDebug(
            "TransactionSignature Debug:\n- Nonce: {Nonce}\n- Timestamp: {Timestamp}" +
            "\n- Payload (first 100 chars): {PayloadSnippet}\n- Computed: {ComputedSignature}\n- Given: {GivenSignature}",
            request.Headers.Nonce,
            request.Headers.Timestamp,
            request.Payload.Substring(0, Math.Min(100, request.Payload.Length)),
            computedSignature,
            request.Headers.Signature);

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(request.Headers.Signature)
        );
    }
}