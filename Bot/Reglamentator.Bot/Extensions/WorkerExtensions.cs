using Reglamentator.Bot.Workers;
namespace Reglamentator.Bot.Extensions;
public static class WorkerExtensions
{
    public static IServiceCollection AddAppWorkers(this IServiceCollection services)
    {
        services.AddHostedService<NotificationWorker>();
        services.AddHostedService<Worker>();
        return services;
    }
}