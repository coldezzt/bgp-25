using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет методы для управления фоновыми задачами операций в Hangfire.
/// </summary>
public interface IHangfireOperationJobHelper
{
    /// <summary>
    /// Создает фоновые задачи для операции и связанных с ней напоминаний.
    /// </summary>
    /// <param name="operation">Операция, для которой создаются задачи.</param>
    void CreateJobsForOperation(Operation operation);
    
    /// <summary>
    /// Обновляет фоновые задачи для операции и связанных с ней напоминаний.
    /// </summary>
    /// <param name="operation">Операция с новыми параметрами, для которой обновляются задачи.</param>
    void UpdateJobsForOperation(Operation operation);
    
    /// <summary>
    /// Удаляет фоновые задачи операции и связанных с ней напоминаний.
    /// </summary>
    /// <param name="operation">Операция, для которой удаляются задачи.</param>
    void DeleteJobsForOperation(Operation operation);
}