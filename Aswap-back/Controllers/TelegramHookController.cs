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
    [HttpPost("api/telegram-webhook")]
    public async Task<IActionResult> Post([FromBody] Update upd)
    {
        if (upd.Message?.Text is not string text) return Ok();

        var cmdEntity = upd.Message.Entities?
            .FirstOrDefault(e => e.Type == MessageEntityType.BotCommand && e.Offset == 0);

        if (cmdEntity is null || !text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            return Ok();

        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) 
            return Ok();

        var token = parts[1];
        var chatId = upd.Message.Chat.Id;
        var tgUser = upd.Message.From?.Username;

        await chatDbCommand.UpdateAccountInfoAsync(token, chatId, tgUser);

        var res = await bot.SendMessage(chatId,
            "✅ Telegram notifications enabled",
            parseMode: ParseMode.Html);

        Console.WriteLine(res);
        return Ok();
    }
}