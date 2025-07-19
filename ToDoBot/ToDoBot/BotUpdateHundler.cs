using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using ToDoBot;

public class BotUpdateHandler : IUpdateHandler
{
    private readonly BotHandler _botHandler;

    public BotUpdateHandler(BotHandler botHandler)
    {
        _botHandler = botHandler;
    }

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        => _botHandler.HandleUpdateAsync(botClient, update, cancellationToken);

    public Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource errorSource, CancellationToken cancellationToken)
        => _botHandler.HandleErrorAsync(botClient, exception, errorSource, cancellationToken);
    public Task HandleErrorAsync(
    ITelegramBotClient botClient,
    Exception exception,
    HandleErrorSource errorSource,
    CancellationToken cancellationToken)
    {
        var errorMessage = exception switch
        {
            Telegram.Bot.Exceptions.ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        Console.WriteLine($"Ошибка бота: {errorMessage}");
        return Task.CompletedTask;
    }

}
