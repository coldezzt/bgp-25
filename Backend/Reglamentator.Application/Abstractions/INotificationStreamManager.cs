using Grpc.Core;

namespace Reglamentator.Application.Abstractions;

public interface INotificationStreamManager<T>
{
    Guid RegisterConsumer(IServerStreamWriter<T> stream);
    void RemoveConsumer(Guid id);
    Task BroadcastNotificationAsync(T notification);
}