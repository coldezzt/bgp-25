using Telegram.Bot;

namespace Reglamentator.Bot.Extensions;
public static class TelegramBotExtensions
{
    public static IServiceCollection AddTelegramBotClient(this IServiceCollection services, string token)
    {
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));
        return services;
    }
}