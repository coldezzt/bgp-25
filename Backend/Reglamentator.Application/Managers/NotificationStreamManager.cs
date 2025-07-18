using System.Collections.Concurrent;
using Grpc.Core;
using Reglamentator.Application.Abstractions;

namespace Reglamentator.Application.Managers;

public class NotificationStreamManager<T>: INotificationStreamManager<T>
{
    private readonly ConcurrentDictionary<Guid, IServerStreamWriter<T>> _streams = [];

    public Guid RegisterConsumer(IServerStreamWriter<T> stream)
    {
        var id = Guid.NewGuid();
        _streams.TryAdd(id, stream);
        return id;
    }

    public void RemoveConsumer(Guid id)
    {
        _streams.TryRemove(id, out _);
    }

    public async Task BroadcastNotificationAsync(T notification)
    {
        foreach (var stream in _streams.Values)
        {
            await stream.WriteAsync(notification);
        }
    }
}