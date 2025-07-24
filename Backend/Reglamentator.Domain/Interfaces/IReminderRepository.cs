using System.Linq.Expressions;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

/// <summary>
/// Предоставляет доступ к данным напоминаний.
/// </summary>
public interface IReminderRepository: IRepository<Reminder>
{
    /// <summary>
    /// Получает напоминание с деталями для обработки в фоновой задаче.
    /// </summary>
    /// <param name="filter">Условие фильтрации напоминаний.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Напоминание с включенными данными связанной операции (Operation)
    /// или null, если напоминание не найдено.
    /// </returns>
    Task<Reminder?> GetWithDetailsForProcessJobAsync(Expression<Func<Reminder, bool>> filter, 
        CancellationToken cancellationToken = default);
}