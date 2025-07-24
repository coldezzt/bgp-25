using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет методы для управления фоновыми задачами напоминаний в Hangfire.
/// </summary>
public interface IHangfireReminderJobHelper
{
    /// <summary>
    /// Создает фоновую задачу для напоминания на основе операции и параметров напоминания.
    /// </summary>
    /// <param name="operation">Операция, для которой создается напоминание.</param>
    /// <param name="reminder">Напоминание, для которого нужно создать задачу.</param>
    void CreateJobForReminder(Operation operation, Reminder reminder);
    
    /// <summary>
    /// Обновляет фоновую задачу для напоминания на основе новых параметров операции и напоминания.
    /// </summary>
    /// <param name="operation">Обновленная операция.</param>
    /// <param name="reminder">Напоминание с новыми параметрами, для которого нужно обновить задачу</param>
    void UpdateJobForReminder(Operation operation, Reminder reminder);
    
    /// <summary>
    /// Удаляет фоновую задачу напоминания.
    /// </summary>
    /// <param name="reminder">Напоминание, для которого нужно удалить задачу.</param>
    void DeleteJobForReminder(Reminder reminder);
}