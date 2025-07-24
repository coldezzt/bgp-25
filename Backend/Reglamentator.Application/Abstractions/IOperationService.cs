using FluentResults;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет функциональность для работы с операциями.
/// </summary>
public interface IOperationService
{
    /// <summary>
    /// Получает запланированные операции пользователя за указанный период.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="range">Временной диапазон для выборки.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо список запланированных операций <see cref="OperationInstance"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи.
    /// </returns>
    Task<Result<List<OperationInstance>>> GetPlanedOperationsAsync(long telegramId, TimeRange range,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает историю выполненных операций пользователя.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо список выполненных операций <see cref="OperationInstance"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи.
    /// </returns>
    Task<Result<List<OperationInstance>>> GetOperationHistoryAsync(long telegramId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает конкретную операцию пользователя.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо операцию <see cref="Operation"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи
    /// </returns>
    Task<Result<Operation>> GetOperationAsync(long telegramId, long operationId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Создает новую операцию для пользователя.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationDto">Данные для создания операции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо созданную операцию <see cref="Operation"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи
    /// </returns>
    Task<Result<Operation>> CreateOperationAsync(long telegramId, CreateOperationDto operationDto,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет существующую операцию пользователя.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationDto">Данные для обновления операции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо обновленную операцию <see cref="Operation"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи
    /// </returns>
    Task<Result<Operation>> UpdateOperationAsync(long telegramId, UpdateOperationDto operationDto,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет операцию пользователя.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат, содержащий либо удалённую операцию <see cref="Operation"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи
    /// </returns>
    Task<Result<Operation>> DeleteOperationAsync(long telegramId, long operationId,
        CancellationToken cancellationToken = default);
}