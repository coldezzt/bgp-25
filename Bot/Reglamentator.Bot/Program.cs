using Reglamentator.Bot;
using Reglamentator.Bot.Services;
using Telegram.Bot;
using Reglamentator.WebAPI;

var builder = Host.CreateApplicationBuilder(args);
var token = builder.Configuration["TelegramBot:Token"];
var grpcUrl = builder.Configuration["Grpc:BackendUrl"];
var channel = Grpc.Net.Client.GrpcChannel.ForAddress(grpcUrl);

builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(token));
builder.Services.AddSingleton(new Operation.OperationClient(channel));
builder.Services.AddSingleton(new Notification.NotificationClient(channel));
builder.Services.AddHostedService<NotificationWorker>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();