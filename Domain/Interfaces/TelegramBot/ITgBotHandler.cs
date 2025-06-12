using Domain.Models.Dtos;
using Telegram.Bot.Types;

namespace Domain.Interfaces.TelegramBot;

public interface ITgBotHandler
{
    Task<Message> NotifyMessageAsync(TgBotDto tgBotDto);
}