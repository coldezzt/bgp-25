using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Managers;
using Reglamentator.Application.Services;

namespace Reglamentator.Application.Extensions;

public static class AddApplicationServicesExt
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IUserService, UserService>();
        
        return services;
    }
    
    public static IServiceCollection AddNotificationManager<T>(this IServiceCollection services)
    {
        services.AddSingleton<INotificationStreamManager<T>, NotificationStreamManager<T>>();
        
        return services;
    }
}