using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Helpers;

/// <summary>
/// Реализация <see cref="IHangfireReminderJobHelper"/> для работы с периодическими задачами напоминаний в Hangfire.
/// </summary>
/// <remarks>
/// Использует <see cref="IRecurringJobManager"/> для управления задачами и <see cref="IServiceProvider"/> для доступа к сервисам.
/// </remarks>
public class HangfireReminderJobHelper(
    IRecurringJobManager recurringJobManager,
    IServiceProvider serviceProvider
    ): IHangfireReminderJobHelper
{
    /// <inheritdoc />
    public void CreateJobForReminder(Operation operation, Reminder reminder)
    {
        if (GetReminderTime(operation, reminder) <= DateTime.UtcNow)
            return;

        recurringJobManager.AddOrUpdate(
            GetReminderJobId(reminder.Id),
            () => ProcessReminderJob(reminder.Id),
            GetReminderCron(operation, reminder));
    }
    
    /// <inheritdoc />
    public void UpdateJobForReminder(Operation operation, Reminder reminder)
    {
        if (GetReminderTime(operation, reminder) <= DateTime.UtcNow)
        {
            DeleteJobForReminder(reminder);
            return;
        }

        recurringJobManager.AddOrUpdate(
            GetReminderJobId(reminder.Id),
            () => ProcessReminderJob(reminder.Id),
            GetReminderCron(operation, reminder));
    }
    
    /// <inheritdoc />
    public void DeleteJobForReminder(Reminder reminder)
    {
        recurringJobManager.RemoveIfExists(GetReminderJobId(reminder.Id));
    }
    
    /// <summary>
    /// Генерирует уникальный идентификатор задачи для напоминания.
    /// </summary>
    /// <param name="reminderId">Идентификатор напоминания.</param>
    /// <returns>Строковый идентификатор в формате "reminder-{reminderId}."</returns>
    private string GetReminderJobId(long reminderId) => $"reminder-{reminderId}";
    
    /// <summary>
    /// Вычисляет время срабатывания напоминания относительно времени операции.
    /// </summary>
    /// <param name="operation">Операция, для которой установлено напоминание.</param>
    /// <param name="reminder">Напоминание с указанием смещения времени.</param>
    /// <returns>
    /// Дата и время, когда должно сработать напоминание
    /// </returns>
    private DateTime GetReminderTime(Operation operation, Reminder reminder) => operation.StartDate - reminder.OffsetBeforeExecution;
    
    /// <summary>
    /// Генерирует cron-выражение для выполнения напоминания.
    /// </summary>
    /// <param name="operation">Операция, связанная с напоминанием.</param>
    /// <param name="reminder">Напоминание для которого создается расписание.</param>
    /// <returns>
    /// Cron-выражение в формате "секунды минуты часы день месяц *",
    /// соответствующее времени срабатывания напоминания
    /// </returns>
    private string GetReminderCron(Operation operation, Reminder reminder)
    {
        var reminderTime = GetReminderTime(operation, reminder);
        return $"{reminderTime.Second} {reminderTime.Minute} {reminderTime.Hour} {reminderTime.Day} {reminderTime.Month} *";
    }
    
    /// <summary>
    /// Обрабатывает задачу напоминания: отправляет уведомление и удаляет задачу.
    /// </summary>
    /// <param name="reminderId">Идентификатор напоминания.</param>
    /// <remarks>
    /// Используется только hangfire
    /// </remarks>
    public async Task ProcessReminderJob(long reminderId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var reminderRepository = scope.ServiceProvider.GetRequiredService<IReminderRepository>();
        var notificationStreamManager = scope.ServiceProvider.GetRequiredService<INotificationStreamManager>();
        
        var reminder = await reminderRepository.GetWithDetailsForProcessJobAsync(r => r.Id == reminderId);
        await notificationStreamManager.BroadcastNotificationAsync(new NotificationResponseDto()
        {
            Message = reminder!.MessageTemplate,
            TelegramId = reminder.Operation.TelegramUserId
        });
        
        DeleteJobForReminder(reminder);
    }
}