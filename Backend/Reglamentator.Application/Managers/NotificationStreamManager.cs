using System.Collections.Concurrent;
using Grpc.Core;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;

namespace Reglamentator.Application.Managers;

public class NotificationStreamManager: INotificationStreamManager
{
    private readonly ConcurrentDictionary<Guid, IServerStreamWriter<NotificationResponseDto>> _streams = [];

    public Guid RegisterConsumer(IServerStreamWriter<NotificationResponseDto> stream)
    {
        var id = Guid.NewGuid();
        _streams.TryAdd(id, stream);
        return id;
    }

    public void RemoveConsumer(Guid id)
    {
        _streams.TryRemove(id, out _);
    }

    public async Task BroadcastNotificationAsync(NotificationResponseDto notification)
    {
        foreach (var stream in _streams.Values)
        {
            await stream.WriteAsync(notification);
        }
    }
}