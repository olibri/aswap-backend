using Domain.Interfaces.TelegramBot;
using Domain.Models.Dtos;
using Domain.Models.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
namespace App.Telegram;

public class TgBotHandler(ITelegramBotClient bot, long adminId) : ITgBotHandler
{
    public async Task<Message> NotifyMessageAsync(TgBotDto dto)
    {
        switch (dto.MessageType)
        {
            case TgMessageType.OrderCreated:
                Console.WriteLine($"[TG] Order created: {dto.OrderId}");
                return await NotifyAccountAsync();
            case TgMessageType.AdminNotification:
                Console.WriteLine($"[TG] Order updated: {dto.OrderId}");
                return await AdminCallAsync(dto);
            default:
                Console.WriteLine($"[TG] Unknown message type: {dto.MessageType}");
                return new Message();
        }
    }

    private async Task<Message> AdminCallAsync(TgBotDto dto)
    {
        var text =
            $"<b>🚨 Потрібно втручання адміна</b>\n\n" +
            $"<b>Order ID:</b> <code>{dto.OrderId}</code>\n" +
            $"<b>Buyer wallet:</b> <code>{dto.BuyerWallet}</code>\n" +
            $"<b>Seller wallet:</b> <code>{dto.SellerWallet}</code>\n\n" +
            $"<a href=\"{dto.OrderUrl}\">🔗 Click</a>";
        try
        {
            var message = await bot.SendMessage(
                chatId: adminId,
                text: text,
                parseMode: ParseMode.Html
            );

            Console.WriteLine($"[TG] Message {message.Dump()}");

            return message;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[TG] send failed: {ex.Message}");
            return new Message();
        }
    }

    private async Task<Message> NotifyAccountAsync()
    {

        throw new NotImplementedException("NotifyAccountAsync is not implemented yet.");
    }
}