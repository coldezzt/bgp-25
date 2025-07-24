using FluentResults;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет функциональность для работы с напоминаниями.
/// </summary>
public interface IReminderService
{
    /// <summary>
    /// Добавляет новое напоминание к операции.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <param name="reminderDto">Данные для создания напоминания.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо созданное напоминание <see cref="Reminder"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи.
    /// </returns>
    Task<Result<Reminder>> AddReminderAsync(long telegramId, long operationId, CreateReminderDto reminderDto,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет существующее напоминание.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <param name="reminderDto">Данные для обновления напоминания.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо обновлённое напоминание <see cref="Reminder"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи.
    /// </returns>
    Task<Result<Reminder>> UpdateReminderAsync(long telegramId, long operationId, UpdateReminderDto reminderDto,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет напоминание.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <param name="reminderId">Идентификатор напоминания.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо удалённое напоминание <see cref="Reminder"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи.
    /// </returns>
    Task<Result<Reminder>> DeleteReminderAsync(long telegramId, long operationId, long reminderId,
        CancellationToken cancellationToken = default);
}