using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

public interface IHangfireOperationJobHelper
{
    void CreateJobsForOperation(Operation operation);
    void UpdateJobsForOperation(Operation operation);
    void DeleteJobsForOperation(Operation operation);
}