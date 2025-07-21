using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using Reglamentator.WebAPI;
using Grpc.Core;

namespace Reglamentator.Bot.Services;

public class NotificationWorker : BackgroundService
{
    private readonly Notification.NotificationClient _notificationClient;
    private readonly ITelegramBotClient _botClient; // Используйте TelegramBotClient

    public NotificationWorker(Notification.NotificationClient notificationClient, ITelegramBotClient botClient)
    {
        _notificationClient = notificationClient;
        _botClient = botClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var request = new NotificationRequest();
        using var call = _notificationClient.ListenForNotifications(request, cancellationToken: stoppingToken);

        try
        {
            while (await call.ResponseStream.MoveNext(stoppingToken))
            {
                var notification = call.ResponseStream.Current;
                try
                {
                    await _botClient.SendMessage(notification.TelegramId, notification.Message, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки уведомления: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка gRPC-стрима уведомлений: {ex.Message}");
        }
    }
}
