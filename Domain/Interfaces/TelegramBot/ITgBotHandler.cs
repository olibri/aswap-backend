using Domain.Models.Api.CoinJelly;
using Domain.Models.Dtos;
using Telegram.Bot.Types;

namespace Domain.Interfaces.TelegramBot;

public interface ITgBotHandler
{
    Task<IReadOnlyList<Message>> NotifyMessageAsync(TgBotDto tgBotDto);

    Task<IReadOnlyList<Message>> NotifyAdminCoinJellyAsync(
      NewUserCoinJellyRequest request);
}