using System.Reflection;
using System.Text;
using Telegram.Bot.Types;

namespace App.Telegram;

public static class TelegramDumpExtensions
{
    public static string Dump(this Message msg)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Message {{ Id = {msg.Id} }}");

        foreach (PropertyInfo p in typeof(Message).GetProperties())
        {
            var val = p.GetValue(msg);
            if (val is null) continue;

            sb.AppendLine($"  {p.Name} = {val}");
        }
        return sb.ToString();
    }
}