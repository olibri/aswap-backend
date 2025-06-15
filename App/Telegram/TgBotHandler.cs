using Domain.Interfaces.Database.Queries;
using Domain.Interfaces.TelegramBot;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace App.Telegram;

public class TgBotHandler(ITelegramBotClient bot, long adminId, IAccountDbQueries accountDbQueries) : ITgBotHandler
{
    public async Task<IReadOnlyList<Message>> NotifyMessageAsync(TgBotDto dto)
    {
        return dto.MessageType switch
        {
            TgMessageType.OrderCreated => await NotifyParticipantsAsync(dto),
            TgMessageType.AdminNotification => await NotifyAdminAsync(dto),
            _ => Array.Empty<Message>()
        };
    }

    private async Task<IReadOnlyList<Message>> NotifyAdminAsync(TgBotDto dto)
    {
        var text =
            $"<b>🚨 Потрібно втручання адміна</b>\n\n" +
            $"<b>Order ID:</b> <code>{dto.DealId}</code>\n" +
            $"<b>Buyer wallet:</b> <code>{dto.BuyerWallet}</code>\n" +
            $"<b>Seller wallet:</b> <code>{dto.SellerWallet}</code>\n\n" +
            $"<a href=\"{dto.OrderUrl}\">🔗 Click</a>";

        return await SendSafeAsync(adminId, text);
    }

    private async Task<IReadOnlyList<Message>> NotifyParticipantsAsync(TgBotDto botDto)
    {
        var text =
            $"<b>✅ Order created</b>\n\n" +
            $"<b>Order ID:</b> <code>{botDto.DealId}</code>\n" +
            $"<b>Buyer wallet:</b> <code>{botDto.BuyerWallet}</code>\n" +
            $"<b>Seller wallet:</b> <code>{botDto.SellerWallet}</code>\n\n" +
            $"<a href=\"{botDto.OrderUrl}\">🔗 Click</a>";

        IEnumerable<string?> wallets = botDto.Receiver switch
        {
            NotificationReceiver.Buyer => new[] { botDto.BuyerWallet },
            NotificationReceiver.Seller => new[] { botDto.SellerWallet },
            _ => new[] { botDto.BuyerWallet, botDto.SellerWallet }
        };

        var chatIds = await accountDbQueries.GetChatIdsAsync(wallets!
            .Where(w => !string.IsNullOrWhiteSpace(w))!);

        return chatIds.Count == 0
            ? Array.Empty<Message>()
            : await SendSafeAsync(chatIds, text);
    }

    private async Task<IReadOnlyList<Message>> SendSafeAsync(
        IEnumerable<long> chatIds, string text)
    {
        var tasks = chatIds.Select(async id =>
        {
            try
            {
                return await bot.SendMessage(id, text, ParseMode.Html);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[TG] send to {id} failed: {ex.Message}");
                return null;
            }
        });

        return (await Task.WhenAll(tasks))
            .Where(m => m != null)
            .ToArray()!;
    }

    private Task<IReadOnlyList<Message>> SendSafeAsync(long id, string text)
    {
        return SendSafeAsync(new[] { id }, text);
    }
}