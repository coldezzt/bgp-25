using AutoMapper;
using Grpc.Core;
using Reglamentator.Application.Abstractions;
using Reglamentator.Domain.Entities;
using Reglamentator.WebAPI.Extensions;

namespace Reglamentator.WebAPI.Services;

public class OperationGrpcService(
    IOperationService operationService,
    IMapper mapper
    ): Operation.OperationBase
{
    public override async Task<OperationHistoryResponse> GetOperationHistory(OperationHistoryRequest request, ServerCallContext context)
    {
        var result = await operationService.GetOperationHistoryAsync(
            request.TelegramId,
            context.CancellationToken);
        
        return new OperationHistoryResponse
        {
            Status = result.ToStatusResponse(),
            History = {result.ToResponseData<OperationInstance, OperationInstanceDto>(mapper)}
        };
    }

    public override async Task<PlanedOperationsResponse> GetPlanedOperations(PlanedOperationsRequest request, ServerCallContext context)
    {
        var result = await operationService.GetPlanedOperationsAsync(
            request.TelegramId, 
            mapper.Map<Domain.Entities.TimeRange>(request.Range),
            context.CancellationToken);
        
        return new PlanedOperationsResponse
        {
            Status = result.ToStatusResponse(),
            Instances = {result.ToResponseData<OperationInstance, OperationInstanceDto>(mapper)}
        };
    }

    public override async Task<OperationResponse> CreateOperation(CreateOperationRequest request, ServerCallContext context)
    {
        var result = await operationService.CreateOperationAsync(
            request.TelegramId,
            mapper.Map<Application.Dtos.CreateOperationDto>(request.Operation),
            context.CancellationToken);

        return new OperationResponse()
        {
            Status = result.ToStatusResponse(),
            Operation = result.ToResponseData<Domain.Entities.Operation, OperationDto>(mapper)
        };
    }

    public override async Task<OperationResponse> UpdateOperation(UpdateOperationRequest request, ServerCallContext context)
    {
        var result = await operationService.UpdateOperationAsync(
            request.TelegramId,
            mapper.Map<Application.Dtos.UpdateOperationDto>(request.Operation),
            context.CancellationToken);

        return new OperationResponse()
        {
            Status = result.ToStatusResponse(),
            Operation = result.ToResponseData<Domain.Entities.Operation, OperationDto>(mapper)
        };
    }

    public override async Task<OperationResponse> DeleteOperation(DeleteOperationRequest request, ServerCallContext context)
    {
        var result = await operationService.DeleteOperationAsync(
            request.TelegramId,
            request.OperationId,
            context.CancellationToken);

        return new OperationResponse()
        {
            Status = result.ToStatusResponse(),
            Operation = result.ToResponseData<Domain.Entities.Operation, OperationDto>(mapper)
        };
    }
}