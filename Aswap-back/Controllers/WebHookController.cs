using System.Reflection;
using System.Security.Cryptography;
using Domain.Models.Api.Hooks;
using Domain.Models.Events;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using Hexarc.Borsh;

namespace Aswap_back.Controllers;

//TODO: Add signature for getting data
[ApiController]
public class WebHookController
    : Controller
{
    [HttpPost]
    [Route("webhook")]
    public async Task<IActionResult> QuickNodeCallback()
    {
        var raw = await new StreamReader(Request.Body).ReadToEndAsync();

        var payload = JsonSerializer.Deserialize<SolanaWebhookPayload>(raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var events = ParseSolanaEvents(payload!.Data[0].Logs);

        foreach (var ev in events)
            Console.WriteLine(ev);

        return Ok();
    }

    private static byte[] Disc(string name)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes($"event:{name}"))[..8];
    }

    private static readonly MethodInfo _deserializeGeneric =
        typeof(BorshSerializer).GetMethod(nameof(BorshSerializer.Deserialize), 1,
            new[] { typeof(byte[]), typeof(BorshSerializerOptions) })!;

   
    static string Hex(ReadOnlySpan<byte> b) => Convert.ToHexString(b);

    static readonly Dictionary<string, Type> _eventMap = new()
    {
        { Hex(Disc("OfferInitialized")),  typeof(OfferInitialized)  },
        { Hex(Disc("EscrowInitialized")), typeof(EscrowInitialized) },
        { Hex(Disc("OfferClaimed")),      typeof(OfferClaimed)      },
        { Hex(Disc("BuyerSigned")),       typeof(BuyerSigned)       },
        { Hex(Disc("SellerSigned")),      typeof(SellerSigned)      },
        { Hex(Disc("FundsReleased")),     typeof(FundsReleased)     },
    };

    private static object? DeserializeRuntime(Type t, ReadOnlySpan<byte> span)
    {
        try
        {
            var method = _deserializeGeneric.MakeGenericMethod(t);
            return method.Invoke(null, new object?[] { span.ToArray(), null });
        }
        catch (TargetInvocationException ex)
        {
            Console.WriteLine($"  ↳ DESERIALIZE FAILED: {ex.InnerException?.Message}");
            return null;
        }
    }


    public static IEnumerable<object> ParseSolanaEvents(string[] logs)
    {
        foreach (var line in logs)
        {
            if (!line.StartsWith("Program data: "))
            {
                Console.WriteLine("» skip - not Program data");
                continue;
            }

            var bytes = Convert.FromBase64String(line["Program data: ".Length..]);
            var disc = bytes.AsSpan(0, 8);
            var key = Hex(disc);

            Console.WriteLine($"» DISC {key}");

            if (!_eventMap.TryGetValue(key, out var type))
            {
                Console.WriteLine("  ↳ unknown discriminator");
                continue;
            }
            Console.WriteLine($"  ↳ type  {type.Name}");

            var payload = bytes.AsSpan(8);
            var obj = DeserializeRuntime(type, payload);

            if (obj is null)
            {
                Console.WriteLine("  ↳ object is null (parse error)");
                continue;
            }

            Console.WriteLine("  ↳ OK");
            yield return obj;
        }
    }

}