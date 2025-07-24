using Reglamentator.Application.Dtos;

namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет методы для управления потоками уведомлений и рассылки сообщений потребителям.
/// </summary>
public interface INotificationStreamManager
{
    /// <summary>
    /// Регистрирует нового потребителя уведомлений и возвращает его идентификатор.
    /// </summary>
    /// <param name="stream">Поток для записи уведомлений.</param>
    /// <returns>Уникальный идентификатор зарегистрированного потребителя.</returns>
    Guid RegisterConsumer(IStreamWriter<NotificationResponseDto> stream);
    
    /// <summary>
    /// Удаляет потребителя уведомлений по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор потребителя для удаления.</param>
    void RemoveConsumer(Guid id);
    
    /// <summary>
    /// Рассылает уведомление всем зарегистрированным потребителям.
    /// </summary>
    /// <param name="notification">Уведомление для рассылки.</param>
    /// <returns>Задача, представляющая асинхронную операцию рассылки.</returns>
    Task BroadcastNotificationAsync(NotificationResponseDto notification);
}