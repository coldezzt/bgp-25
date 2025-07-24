using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Data.Repositories;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Extensions;

public static class AddDbContextExt
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IOperationInstanceRepository, OperationInstanceRepository>();
        services.AddScoped<IReminderRepository, ReminderRepository>();
        services.AddScoped<ITelegramUserRepository, TelegramUserRepository>();
        
        return services.AddDbContext<AppDbContext>(builder =>
        {
            builder.UseNpgsql(configuration["Database:ConnectionString"]);
            builder.UseSnakeCaseNamingConvention();
        });
    }
}