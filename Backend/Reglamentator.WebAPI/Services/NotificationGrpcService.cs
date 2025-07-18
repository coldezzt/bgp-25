using Grpc.Core;
using Reglamentator.Application.Abstractions;

namespace Reglamentator.WebAPI.Services;

public class NotificationGrpcService(
    INotificationStreamManager<NotificationResponse> notificationStreamManager
    ): Notification.NotificationBase
{

    public override async Task ListenForNotifications(
        NotificationRequest request,
        IServerStreamWriter<NotificationResponse> responseStream,
        ServerCallContext context)
    {
        var id = notificationStreamManager.RegisterConsumer(responseStream);

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(15), context.CancellationToken);
            }
        }
        catch (TaskCanceledException) { }
        finally
        {
            notificationStreamManager.RemoveConsumer(id);
        }
    }
}