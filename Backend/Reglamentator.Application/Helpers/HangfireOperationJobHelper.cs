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
        if (operation.NextOperationInstance == null) 
            return;
        
        recurringJobManager.AddOrUpdate(
            GetOperationJobId(operation.Id),
            () => ProcessOperationJob(operation.Id),
            GetOperationCron(operation));
            
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.CreateJobForReminder(operation, reminder);
        }
    }
    
    public void UpdateJobsForOperation(Operation operation)
    {
        if (operation.NextOperationInstance == null) 
            return;
        
        recurringJobManager.AddOrUpdate(
            GetOperationJobId(operation.Id),
            () => ProcessOperationJob(operation.Id),
            GetOperationCron(operation));
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.UpdateJobForReminder(operation, reminder);
        }
    }

    public void DeleteJobsForOperation(Operation operation)
    {
        recurringJobManager.RemoveIfExists(GetOperationJobId(operation.Id));
        
        foreach (var reminder in operation.Reminders)
        {
            hangfireReminderJobHelper.DeleteJobForReminder(reminder);
        }
    }
    
    private string GetOperationJobId(long operationId) => $"operation-{operationId}";
    
    private string GetOperationCron(Operation operation)
    {
        var startDate = operation.StartDate;
        return $"{startDate.Minute} {startDate.Hour} {startDate.Day} {startDate.Month} *";
    }

    private void ProcessPastOperation(OperationInstance operationInstance)
    {
        operationInstance.ExecutedAt = DateTime.UtcNow;
        operationInstance.Result = "Done";
    }
    
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
    
    private DateTime GetNextOccurrence(Operation operation)
    {
        var cronExpression = CrontabSchedule.Parse(operation.Cron);
        var nextOccurrence = cronExpression.GetNextOccurrence(operation.StartDate);
        
        return nextOccurrence;
    }
    
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