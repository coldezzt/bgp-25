using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Helpers;

public class HangfireReminderJobHelper(
    IRecurringJobManager recurringJobManager,
    IServiceProvider serviceProvider
    ): IHangfireReminderJobHelper
{
    public void CreateJobForReminder(Operation operation, Reminder reminder)
    {
        if (GetReminderTime(operation, reminder) <= DateTime.Now)
            return;

        recurringJobManager.AddOrUpdate(
            GetReminderJobId(reminder.Id),
            () => ProcessReminderJob(reminder.Id),
            GetReminderCron(operation, reminder));
    }

    public void UpdateJobForReminder(Operation operation, Reminder reminder)
    {
        if (GetReminderTime(operation, reminder) <= DateTime.Now)
        {
            DeleteJobForReminder(reminder);
            return;
        }

        recurringJobManager.AddOrUpdate(
            GetReminderJobId(reminder.Id),
            () => ProcessReminderJob(reminder.Id),
            GetReminderCron(operation, reminder));
    }
    
    public void DeleteJobForReminder(Reminder reminder)
    {
        recurringJobManager.RemoveIfExists(GetReminderJobId(reminder.Id));
    }
    
    private string GetReminderJobId(long reminderId) => $"reminder-{reminderId}";
    
    private DateTime GetReminderTime(Operation operation, Reminder reminder) => operation.StartDate - reminder.OffsetBeforeExecution;
    
    private string GetReminderCron(Operation operation, Reminder reminder)
    {
        var reminderTime = GetReminderTime(operation, reminder);
        return $"{reminderTime.Minute} {reminderTime.Hour} {reminderTime.Day} {reminderTime.Month} *";
    }
    
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