using SimpleBase;
using System.Text;

namespace Domain.Models.Events.Helper;

public static class ConvertHelper
{
    public static string ToBase58(byte[] bytes) =>
        Base58.Bitcoin.Encode(bytes);


    public static long ToUnixSeconds(DateTime dt) =>
        new DateTimeOffset(dt).ToUnixTimeSeconds();

    public static DateTime FromUnixSeconds(long ts) =>
        DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;

   
    public static string Fiat(byte[] code) =>
        Encoding.ASCII.GetString(code).TrimEnd('\0');

}