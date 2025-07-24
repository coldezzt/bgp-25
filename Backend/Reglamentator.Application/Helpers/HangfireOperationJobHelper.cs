using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using NCrontab;
using Reglamentator.Application.Abstractions;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Helpers;

/// <summary>
/// Реализация <see cref="IHangfireOperationJobHelper"/> для управления задачами операций в Hangfire.
/// </summary>
/// <remarks>
/// Использует:
/// <list type="bullet">
///   <item><see cref="IHangfireReminderJobHelper"/> для управления задачами напоминаний.</item>
///   <item><see cref="IRecurringJobManager"/> для работы с Hangfire.</item>
///   <item><see cref="IServiceProvider"/> для доступа к репозиториям.</item>
/// </list>
/// </remarks>
public class HangfireOperationJobHelper(
    IHangfireReminderJobHelper hangfireReminderJobHelper,
    IRecurringJobManager recurringJobManager,
    IServiceProvider serviceProvider
    ): IHangfireOperationJobHelper
{
    /// <inheritdoc />
    public void CreateJobsForOperation(Operation operation)
    {
        if (operation.NextOperationInstance == null) 
            return;
        
        recurringJobManager.AddOrUpdate(
            GetOperationJobId(operation.Id),
            () => ProcessOperationJob(operation.Id),
            GetOperationCron(operation));
        
        if (operation.Reminders == null)
            return;
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.CreateJobForReminder(operation, reminder);
        }
    }
    
    /// <inheritdoc />
    public void UpdateJobsForOperation(Operation operation)
    {
        if (operation.NextOperationInstance == null) 
            return;
        
        recurringJobManager.AddOrUpdate(
            GetOperationJobId(operation.Id),
            () => ProcessOperationJob(operation.Id),
            GetOperationCron(operation));
        
        if (operation.Reminders == null)
            return;
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.UpdateJobForReminder(operation, reminder);
        }
    }

    /// <inheritdoc />
    public void DeleteJobsForOperation(Operation operation)
    {
        recurringJobManager.RemoveIfExists(GetOperationJobId(operation.Id));
        
        if (operation.Reminders == null)
            return;
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.DeleteJobForReminder(reminder);
        }
    }
    
    /// <summary>
    /// Генерирует уникальный идентификатор задачи для операции.
    /// </summary>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <returns>Строка в формате "operation-{operationId}."</returns>
    private string GetOperationJobId(long operationId) => $"operation-{operationId}";
    
    /// <summary>
    /// Формирует cron-выражение на основе даты начала операции
    /// </summary>
    /// <param name="operation">Операция, для которой создается расписание</param>
    /// <returns>Cron-выражение в формате "секунды минуты часы день месяц *"</returns>
    private string GetOperationCron(Operation operation)
    {
        var startDate = operation.StartDate;
        return $"{startDate.Second} {startDate.Minute} {startDate.Hour} {startDate.Day} {startDate.Month} *";
    }

    /// <summary>
    /// Помечает экземпляр операции как выполненный в текущий момент времени.
    /// </summary>
    /// <param name="operationInstance">Экземпляр операции для обработки.</param>
    private void ProcessPastOperation(OperationInstance operationInstance)
    {
        operationInstance.ExecutedAt = DateTime.UtcNow;
        operationInstance.Result = "Done";
    }
    
    /// <summary>
    /// Создает следующий экземпляр выполнения для периодической операции.
    /// </summary>
    /// <param name="operation">Операция для обработки.</param>
    /// <remarks>
    /// Устанавливает новую дату выполнения на основе cron-выражения
    /// и создает новый экземпляр операции в истории.
    /// </remarks>
    private void ProcessCronOperationCreation(Operation operation)
    {
        var now = DateTime.UtcNow;
        operation.StartDate = GetNextOccurrence(operation);

        var newOperationInstance = new OperationInstance
        {
            ScheduledAt = now,
            OperationId = operation.Id,
            Result = null,
            ExecutedAt = null
        };
        
        operation.NextOperationInstance = newOperationInstance;
    }
    
    /// <summary>
    /// Вычисляет следующую дату выполнения для периодической операции
    /// </summary>
    /// <param name="operation">Операция с cron-выражением</param>
    /// <returns>Дата следующего выполнения</returns>
    private DateTime GetNextOccurrence(Operation operation)
    {
        var cronExpression = CrontabSchedule.Parse(
            operation.Cron, 
            new CrontabSchedule.ParseOptions {IncludingSeconds = true});
        var nextOccurrence = cronExpression.GetNextOccurrence(operation.StartDate);
        
        return nextOccurrence;
    }
    
    /// <summary>
    /// Обрабатывает задачу операции: завершает текущий экземпляр, создает следующий (для Cron) или удаляет задачу.
    /// </summary>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <remarks>
    /// Используется только hangfire
    /// </remarks>
    public async Task ProcessOperationJob(long operationId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var operationRepository = scope.ServiceProvider.GetRequiredService<IOperationRepository>();
        
        var operation = await operationRepository.GetWithDetailsForProcessJobAsync(
            op => op.Id == operationId);

        ProcessPastOperation(operation!.NextOperationInstance!);
        operation.NextOperationInstance = null;
        
        if (operation.Cron != null)
        {
            ProcessCronOperationCreation(operation);
            CreateJobsForOperation(operation);
        }
        else
        {
            DeleteJobsForOperation(operation);
        }
        
        await operationRepository.UpdateEntityAsync(operation);
    }
}