using Grpc.Core;
using Reglamentator.Application.Abstractions;
using Reglamentator.WebAPI.Mapping.Adapters;

namespace Reglamentator.WebAPI.Services;

public class NotificationGrpcService(
    INotificationStreamManager notificationStreamManager
    ): Notification.NotificationBase
{
    public override async Task ListenForNotifications(
        NotificationRequest request,
        IServerStreamWriter<NotificationResponse> responseStream,
        ServerCallContext context)
    {
        var adapter = new GrpcNotificationStreamAdapter(responseStream);
        var id = notificationStreamManager.RegisterConsumer(adapter);

        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(15), context.CancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            //ignored
        }
        finally
        {
            notificationStreamManager.RemoveConsumer(id);
        }
    }
}