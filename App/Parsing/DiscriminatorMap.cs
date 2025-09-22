using Domain.Models.Events;
using System.Security.Cryptography;
using System.Text;

namespace App.Parsing;

public static class DiscriminatorMap
{
    private static byte[] Disc(string name) =>
        SHA256.HashData(Encoding.UTF8.GetBytes($"event:{name}"))[..8];

    private static readonly Dictionary<string, Type> Map = new()
    {
        { Hex(Disc("OfferInitialized")),  typeof(UniversalOrderCreated)  },
        { Hex(Disc("EscrowInitialized")), typeof(EscrowInitialized) },
        { Hex(Disc("OfferClaimed")),      typeof(OfferClaimed)      },
        { Hex(Disc("BuyerSigned")),       typeof(BuyerSigned)       },
        { Hex(Disc("SellerSigned")),      typeof(SellerSigned)      },
        { Hex(Disc("FundsReleased")),     typeof(FundsReleased)     },
    };

    public static bool TryGetType(ReadOnlySpan<byte> disc, out Type? type) =>
        Map.TryGetValue(Hex(disc), out type);

    private static string Hex(ReadOnlySpan<byte> b) => Convert.ToHexString(b);

}