using Domain.Interfaces.Hooks.Parsing;
using Hexarc.Borsh;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace App.Parsing;

public sealed class AnchorAnchorEventParser(ILogger<AnchorAnchorEventParser> log) : IAnchorEventParser
{
    private static readonly MethodInfo deserialize =
        typeof(BorshSerializer).GetMethod(nameof(BorshSerializer.Deserialize), 1,
            new[] { typeof(byte[]), typeof(BorshSerializerOptions) })!;

    public IEnumerable<IAnchorEvent> Parse(string[] logs)
    {
        if (logs == null)
        {
            log.LogWarning("Parse called with null logs");
            yield break;
        }

        var programDataLines = 0;

        foreach (var line in logs)
        {
            if (!line.StartsWith("Program data: "))
                continue;

            programDataLines++;

            var b64 = line["Program data: ".Length..].Trim();
            byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(b64);
            }
            catch (Exception ex)
            {
                log.LogWarning(ex, "Failed to base64-decode Program data line. Length={Len}, Head='{Head}'",
                    b64.Length, b64[..Math.Min(32, b64.Length)]);
                continue;
            }

            if (bytes.Length < 8)
            {
                log.LogWarning("Program data payload too short: {Len} bytes (need >= 8)", bytes.Length);
                continue;
            }

            var disc = bytes.AsSpan(0, 8);
            var discHex = Convert.ToHexString(disc);

            if (!DiscriminatorMap.TryGetType(disc, out var evtType) || evtType is null)
            {
                log.LogWarning("Unknown discriminator {Disc}. PayloadLen={Len}", discHex, bytes.Length);
                continue;
            }

            log.LogInformation("Matched discriminator {Disc} -> {Type}. PayloadLen={Len}", discHex, evtType.FullName, bytes.Length - 8);

            var obj = Deserialize(evtType, bytes.AsSpan(8));
            if (obj != null)
            {
                log.LogInformation("Deserialized {Type} successfully", obj.GetType().Name);
                yield return obj;
            }
            else
            {
                log.LogError("Failed to deserialize payload as {Type}. Discriminator={Disc}, PayloadLen={Len}", evtType.FullName, discHex, bytes.Length - 8);
            }
        }

        if (programDataLines == 0)
        {
            log.LogWarning("No 'Program data:' lines found among {Count} log lines", logs.Length);
        }
    }

    private IAnchorEvent? Deserialize(Type t, ReadOnlySpan<byte> payload)
    {
        try
        {
            var m = deserialize.MakeGenericMethod(t);
            return (IAnchorEvent?)m.Invoke(null, new object?[] { payload.ToArray(), null });
        }
        catch (TargetInvocationException tie)
        {
            log.LogError(tie.InnerException ?? tie, "Borsh deserialization threw for {Type}. PayloadLen={Len}", t.FullName, payload.Length);
            return null;
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Failed to deserialize {Type}. PayloadLen={Len}", t.FullName, payload.Length);
            return null;
        }
    }
}