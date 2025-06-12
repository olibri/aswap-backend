using App.Strategy;
using Domain.Interfaces.Hooks.Parsing;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Domain.Interfaces.Chat;
using Telegram.Bot;

namespace Aswap_back.Controllers;

[ApiController]
public class TelegramHookController(
    IChatDbCommand chatDbCommand,
    ITelegramBotClient bot,
    ILogger<WebHookController> log)
    : Controller
{
    [HttpPost("telegram-webhook")]
    public async Task<IActionResult> Post([FromBody] Update upd)
    {
        if (upd.Message is { Text: { } text } msg)
        {
            if (text.StartsWith("/start"))
            {
                var token = text.Length > 6 ? text.Substring(7) : null;
                if (!string.IsNullOrWhiteSpace(token))
                {
                    await chatDbCommand.UpdateAccountInfo(token, msg.Chat.Id);
                    await bot.SendMessage(msg.Chat.Id,
                        "✅ Telegram notifications enabled",
                        parseMode: ParseMode.Html);
                }
            }
        }
        return Ok();
    }
}