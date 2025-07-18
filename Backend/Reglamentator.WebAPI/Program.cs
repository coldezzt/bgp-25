using Reglamentator.Application.Extensions;
using Reglamentator.Data.Extensions;
using Reglamentator.WebAPI.Extensions;
using Reglamentator.WebAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddGrpc();

builder.Services.AddAutoMapperWithConfigure();
builder.Services.AddApplicationServices();
builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddDbContext(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<OperationGrpcService>();
app.MapGrpcService<UserGrpcService>();
app.MapGrpcService<NotificationGrpcService>();
app.MapGrpcService<ReminderGrpcService>();

app.Run();