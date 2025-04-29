using Autofac.Features.Indexed;
using Domain.Interfaces.Hooks;
using Domain.Models;
using Domain.Models.Api.Hooks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace App.WebhookSettings;

public sealed class WebhookProcessor(
    ILogger<WebhookProcessor> logger,
    IIndex<string, ITransactionParser> transactionParsers)
    : IWebhookProcessor
{
    public async Task<Result<string>> ProcessWebhook(WebhookRequest request)
    {
        var transactionParser = ValidateWebhookRequest(request);
        //var parsedTransaction = ParseAndValidateTransaction(transactionParser, request.Payload);
        //return await transactionProcessor.ProcessTransactionAsync(parsedTransaction);
        throw new NotImplementedException("Transaction processing not implemented yet");
    }

    private ITransactionParser ValidateWebhookRequest(WebhookRequest request)
    {
        if (!request.SignatureValidator.VerifySignature(request))
        {
            logger.LogError("TransactionSignature verification failed");
            throw new WebhookException("Invalid signature id", StatusCodes.Status401Unauthorized);
        }

        if (!transactionParsers.TryGetValue(request.StreamId, out var transactionParser))
        {
            logger.LogError("No transaction parser found for stream id: {StreamId}", request.StreamId);
            throw new WebhookException("No transaction parser for given stream", StatusCodes.Status404NotFound);
        }

        return transactionParser;
    }
    //private ParsedTransaction ParseAndValidateTransaction(ITransactionParser transactionParser, string payload)
    //{
    //    var parsedTransaction = transactionParser.ParseTransaction(payload);

    //    if (parsedTransaction == null)
    //    {
    //        logger.LogError("Unable to parse transaction payload");

    //        //Should return 202 status code, else the webhook provider will drop
    //        throw new WebhookException("Unable to parse transaction payload", StatusCodes.Status202Accepted);
    //    }

    //    return parsedTransaction;
    //}
}