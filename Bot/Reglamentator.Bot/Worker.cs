namespace Reglamentator.Bot;

using Telegram.Bot;
using Reglamentator.WebAPI;

public class Worker : BackgroundService
{
    private readonly TelegramBotService _botService;

    public Worker(ITelegramBotClient botClient, Operation.OperationClient grpcClient)
    {
        _botService = new TelegramBotService(botClient, grpcClient);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _botService.StartAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}