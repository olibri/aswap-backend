using System.Text;
using SimpleBase;

namespace Domain.Models.Events.Helper;

public static class EventHelper
{
    public static string ToBase58(byte[] bytes) =>
        Base58.Bitcoin.Encode(bytes);

    public static string Fiat(byte[] code) =>
        Encoding.ASCII.GetString(code).TrimEnd('\0');
}