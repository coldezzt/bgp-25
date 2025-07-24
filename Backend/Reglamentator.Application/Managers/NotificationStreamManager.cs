using System.Collections.Concurrent;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;

namespace Reglamentator.Application.Managers;

/// <summary>
/// Реализация <see cref="INotificationStreamManager"/> для управления потоками уведомлений.
/// </summary>
/// <remarks>
/// Использует потокобезопасную коллекцию для хранения зарегистрированных потребителей.
/// </remarks>
public class NotificationStreamManager: INotificationStreamManager
{
    private readonly ConcurrentDictionary<Guid, IStreamWriter<NotificationResponseDto>> _streams = [];

    /// <inheritdoc />
    public Guid RegisterConsumer(IStreamWriter<NotificationResponseDto> stream)
    {
        var id = Guid.NewGuid();
        _streams.TryAdd(id, stream);
        return id;
    }

    /// <inheritdoc />
    public void RemoveConsumer(Guid id)
    {
        _streams.TryRemove(id, out _);
    }

    /// <inheritdoc />
    public async Task BroadcastNotificationAsync(NotificationResponseDto notification)
    {
        foreach (var stream in _streams.Values)
        {
            await stream.WriteAsync(notification);
        }
    }
}