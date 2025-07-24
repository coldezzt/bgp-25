using Grpc.Net.Client;


namespace Reglamentator.Bot.Extensions;
public static class GrpcExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services, string grpcUrl)
    {
        var channel = GrpcChannel.ForAddress(grpcUrl);
        services.AddSingleton(new Operation.OperationClient(channel));
        services.AddSingleton(new Notification.NotificationClient(channel));
        services.AddSingleton(new Reminder.ReminderClient(channel));
        services.AddSingleton(new User.UserClient(channel));
        return services;
    }
}