using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

/// <summary>
/// Предоставляет доступ к данным экземплярам операций.
/// </summary>
public interface IOperationInstanceRepository: IRepository<OperationInstance>
{
    /// <summary>
    /// Получает список выполненных операций для указанного пользователя.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Список выполненных операций, отсортированных по дате выполнения.
    /// Каждая операция включает связанные данные <see cref="Operation"/>.
    /// </returns>
    Task<List<OperationInstance>> GetExecutedUserOperationsAsync(long telegramId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает список запланированных операций пользователя за указанный период.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя в Telegram.</param>
    /// <param name="range">Временной диапазон для выборки (День/Неделя/Месяц).</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Список запланированных операций, отсортированных по дате начала.
    /// Каждая операция включает связанные данные Operation.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Возникает при передаче неподдерживаемого значения TimeRange
    /// </exception>
    /// <remarks>
    /// Для каждого временного диапазона рассчитываются границы:
    /// <list type="bullet">
    ///   <item><description>День - с 00:00 до 23:59 текущего дня</description></item>
    ///   <item><description>Неделя - с понедельника по воскресенье текущей недели</description></item>
    ///   <item><description>Месяц - с 1 по последнее число текущего месяца</description></item>
    /// </list>
    /// Выбираются только операции с датой начала в пределах диапазона.
    /// </remarks>
    Task<List<OperationInstance>> GetPlanedUserOperationsAsync(long telegramId, TimeRange range, 
        CancellationToken cancellationToken = default);
}