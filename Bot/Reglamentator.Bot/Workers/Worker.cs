using Reglamentator.Bot.Services;
using Telegram.Bot;

namespace Reglamentator.Bot.Workers;
public class Worker : BackgroundService
{
    private readonly TelegramBotService _botService;

    public Worker(ITelegramBotClient botClient, Operation.OperationClient grpcClient, Reminder.ReminderClient reminderClient, User.UserClient userClient)
    {
        _botService = new TelegramBotService(botClient, grpcClient, reminderClient, userClient);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _botService.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}