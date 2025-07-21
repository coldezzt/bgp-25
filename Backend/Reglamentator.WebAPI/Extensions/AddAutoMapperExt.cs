using System.Reflection;

namespace Reglamentator.WebAPI.Extensions;

public static class AddAutoMapperExt
{
    public static IServiceCollection AddAutoMapperWithConfigure(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => {}, Assembly.GetExecutingAssembly());
        
        return services;
    }
}