using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using NCrontab;
using Reglamentator.Application.Abstractions;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Helpers;

public class HangfireOperationJobHelper(
    IHangfireReminderJobHelper hangfireReminderJobHelper,
    IRecurringJobManager recurringJobManager,
    IServiceProvider serviceProvider
    ): IHangfireOperationJobHelper
{
    public void CreateJobsForOperation(Operation operation)
    {
        if (operation.Cron != null)
        {
            recurringJobManager.AddOrUpdate(
                GetOperationJobId(operation.Id),
                () => ProcessOperationJob(operation.Id),
                operation.Cron);
        }
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.CreateJobForReminder(operation, reminder);
        }
    }
    
    public void UpdateJobsForOperation(Operation operation)
    {
        if (operation.Cron != null)
        {
            recurringJobManager.AddOrUpdate(
                GetOperationJobId(operation.Id),
                () => ProcessOperationJob(operation.Id),
                operation.Cron);
        }
        else
        {
            DeleteJobForOperation(operation.Id);
        }
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.UpdateJobForReminder(operation, reminder);
        }
    }

    public void DeleteJobsForOperation(Operation operation)
    {
        DeleteJobForOperation(operation.Id);
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.DeleteJobForReminder(reminder);
        }
    }

    private void DeleteJobForOperation(long operationId)
    {
        recurringJobManager.RemoveIfExists(GetOperationJobId(operationId));
    }
    
    private string GetOperationJobId(long operationId) => $"operation-{operationId}";
    
    public async Task ProcessOperationJob(long operationId)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var operationRepository = scope.ServiceProvider.GetRequiredService<IOperationRepository>();
        
        var operation = await operationRepository.GetWithDetailsForProcessJobAsync(
            op => op.Id == operationId);
        
        var now = DateTime.UtcNow;
        var cronExpression = CrontabSchedule.Parse(operation!.Cron);
        var nextOccurrence = cronExpression.GetNextOccurrence(operation.StartDate);
        operation.StartDate = nextOccurrence;
        operation.NextOperationInstance.ExecutedAt = now;
        operation.NextOperationInstance.Result = "Done";

        var newOperationInstance = new OperationInstance
        {
            ScheduledAt = now,
            OperationId = operationId,
            Result = null,
            ExecutedAt = null
        };
        
        operation.NextOperationInstance = newOperationInstance;
        
        await operationRepository.UpdateEntityAsync(operation);

        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.CreateJobForReminder(operation, reminder);
        }
    }
}