using Reglamentator.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<OperationGrpcService>();
app.MapGrpcService<UserGrpcService>();
app.MapGrpcService<NotificationGrpcService>();

app.Run();