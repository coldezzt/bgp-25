using System.Linq.Expressions;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

/// <summary>
/// Предоставляет доступ к данным операций.
/// </summary>
public interface IOperationRepository: IRepository<Operation>
{
    /// <summary>
    /// Получает операцию с деталями для обработки в фоновой задаче.
    /// </summary>
    /// <param name="filter">Условие фильтрации операций.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Операция с включенными данными:
    /// <list type="bullet">
    ///   <item>Следующий экземпляр выполнения NextOperationInstance</item>
    ///   <item>Список напоминаний Reminders</item>
    /// </list>
    /// или null, если операция не найдена.
    /// </returns>
    Task<Operation?> GetWithDetailsForProcessJobAsync(Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает операцию с привязанными напоминаниями.
    /// </summary>
    /// <param name="filter">Условие фильтрации операций.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// Операция с включенным списком напоминаний Reminders
    /// или null, если операция не найдена.
    /// </returns>
    Task<Operation?> GetWithRemindersAsync(Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default);
}