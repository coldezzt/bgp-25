using Grpc.Core;

namespace Reglamentator.WebAPI.Services;

public class OperationGrpcService: Operation.OperationBase
{
    public override Task<OperationHistoryResponse> GetOperationHistory(OperationHistoryRequest request, ServerCallContext context)
    {
        return base.GetOperationHistory(request, context);
    }

    public override Task<PlanedOperationsResponse> GetPlanedOperations(PlanedOperationsRequest request, ServerCallContext context)
    {
        return base.GetPlanedOperations(request, context);
    }

    public override Task<OperationResponse> CreateOperation(CreateOperationRequest request, ServerCallContext context)
    {
        return base.CreateOperation(request, context);
    }

    public override Task<OperationResponse> UpdateOperation(UpdateOperationRequest request, ServerCallContext context)
    {
        return base.UpdateOperation(request, context);
    }

    public override Task<OperationResponse> DeleteOperation(DeleteOperationRequest request, ServerCallContext context)
    {
        return base.DeleteOperation(request, context);
    }
}