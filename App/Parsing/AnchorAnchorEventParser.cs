using Hexarc.Borsh;
using System.Reflection;
using Domain.Interfaces.Hooks.Parsing;

namespace App.Parsing;

public sealed class AnchorAnchorEventParser : IAnchorEventParser
{
    private static readonly MethodInfo deserialize =
        typeof(BorshSerializer).GetMethod(nameof(BorshSerializer.Deserialize), 1,
            new[] { typeof(byte[]), typeof(BorshSerializerOptions) })!;

    public IEnumerable<IAnchorEvent> Parse(string[] logs)
    {
        foreach (var line in logs)
        {
            if (!line.StartsWith("Program data: ")) continue;

            var bytes = Convert.FromBase64String(line["Program data: ".Length..]);
            if (!DiscriminatorMap.TryGetType(bytes.AsSpan(0, 8), out var evtType)) continue;

            var obj = Deserialize(evtType!, bytes.AsSpan(8));
            if (obj != null) yield return obj;
        }
    }

    private static IAnchorEvent? Deserialize(Type t, ReadOnlySpan<byte> payload)
    {
        try
        {
            var m = deserialize.MakeGenericMethod(t);
            return (IAnchorEvent?)m.Invoke(null, new object?[] { payload.ToArray(), null });
        }
        catch
        {
            return null;
        }
    }
}