using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Reglamentator.WebAPI.Validation;

public class ValidationInterceptor(IServiceProvider serviceProvider)
    : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var validator = serviceProvider.GetService<IValidator<TRequest>>();

        if (validator == null)
            return await continuation(request, context);
        
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.IsValid) 
            return await continuation(request, context);
        
        var status = new Status(
            StatusCode.InvalidArgument,
            $"Невалидные данные: {string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage))}");
                
        throw new RpcException(status);
    }
}