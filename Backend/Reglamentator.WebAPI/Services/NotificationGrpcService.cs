using Grpc.Core;

namespace Reglamentator.WebAPI.Services;

public class NotificationGrpcService: Notification.NotificationBase
{
    public override Task ListenForNotifications(NotificationRequest request, IServerStreamWriter<NotificationResponse> responseStream, ServerCallContext context)
    {
        return base.ListenForNotifications(request, responseStream, context);
    }
}