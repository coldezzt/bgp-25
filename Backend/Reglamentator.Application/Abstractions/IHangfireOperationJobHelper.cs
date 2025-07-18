using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

public interface IHangfireOperationJobHelper
{
    public void CreateJobsForOperation(Operation operation);
    public void UpdateJobsForOperation(Operation operation);
    public void DeleteJobsForOperation(Operation operation);
}