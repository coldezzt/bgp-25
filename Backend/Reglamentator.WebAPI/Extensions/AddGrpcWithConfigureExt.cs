using Reglamentator.WebAPI.Validation;

namespace Reglamentator.WebAPI.Extensions;

public static class AddGrpcWithConfigureExt
{
    public static IServiceCollection AddGrpcWithConfigure(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ValidationInterceptor>();
        });
        
        return services;
    }
}