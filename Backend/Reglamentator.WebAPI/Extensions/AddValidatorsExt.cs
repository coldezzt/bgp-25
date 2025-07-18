using FluentValidation;

namespace Reglamentator.WebAPI.Extensions;

public static class AddValidatorsExt
{
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        
        return services;
    }
}