using Reglamentator.Application.Dtos;

namespace Reglamentator.Application.Abstractions;

public interface INotificationStreamManager
{
    Guid RegisterConsumer(IStreamWriter<NotificationResponseDto> stream);
    void RemoveConsumer(Guid id);
    Task BroadcastNotificationAsync(NotificationResponseDto notification);
}