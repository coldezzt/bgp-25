using Reglamentator.Bot.Extensions;

var builder = Host.CreateApplicationBuilder(args);
var token = builder.Configuration.GetRequiredValue("TelegramBot:Token");
var grpcUrl = builder.Configuration.GetRequiredValue("Grpc:BackendUrl");

try
{
    builder.Services
        .AddGrpcClients(grpcUrl)
        .AddTelegramBotClient(token)
        .AddAppWorkers();

    var host = builder.Build();
    await host.RunAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to initialize services: {ex.Message}");
}