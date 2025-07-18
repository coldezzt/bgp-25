using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

public interface IOperationInstanceRepository: IRepository<OperationInstance>
{
    Task<List<OperationInstance>> GetExecutedUserOperationsAsync(long telegramId,
        CancellationToken cancellationToken = default);
    Task<List<OperationInstance>> GetPlanedUserOperationsAsync(long telegramId, TimeRange range, 
        CancellationToken cancellationToken = default);
}