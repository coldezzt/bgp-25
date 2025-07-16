using Reglamentator.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<OperationService>();
app.MapGrpcService<UserService>();

app.Run();