using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Helpers;
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
        services.AddApplicationHelpers();
        services.AddApplicationManagers();
        
        return services;
    }

    private static IServiceCollection AddApplicationManagers(this IServiceCollection services)
    {
        services.AddSingleton<INotificationStreamManager, NotificationStreamManager>();
        
        return services;
    }

    private static IServiceCollection AddApplicationHelpers(this IServiceCollection services)
    {
        services.AddSingleton<IHangfireReminderJobHelper, HangfireReminderJobHelper>();
        services.AddSingleton<IHangfireOperationJobHelper, HangfireOperationJobHelper>();
        
        return services;
    }
}