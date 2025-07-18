using FluentResults;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

public interface IOperationService
{
    Task<Result<List<OperationInstance>>> GetPlanedOperationsAsync(long telegramId, TimeRange range,
        CancellationToken cancellationToken = default);
    Task<Result<List<OperationInstance>>> GetOperationHistoryAsync(long telegramId,
        CancellationToken cancellationToken = default);
    Task<Result<Operation>> CreateOperationAsync(long telegramId, CreateOperationDto operationDto,
        CancellationToken cancellationToken = default);
    Task<Result<Operation>> UpdateOperationAsync(long telegramId, UpdateOperationDto operationDto,
        CancellationToken cancellationToken = default);
    Task<Result<Operation>> DeleteOperationAsync(long telegramId, long operationId,
        CancellationToken cancellationToken = default);
}