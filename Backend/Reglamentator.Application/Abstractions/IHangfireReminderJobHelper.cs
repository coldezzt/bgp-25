using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

public interface IHangfireReminderJobHelper
{
    void CreateJobForReminder(Operation operation, Reminder reminder);
    void UpdateJobForReminder(Operation operation, Reminder reminder);
    void DeleteJobForReminder(Reminder reminder);
}