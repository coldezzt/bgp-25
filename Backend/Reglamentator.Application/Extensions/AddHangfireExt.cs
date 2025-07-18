using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reglamentator.Application.Extensions;

public static class AddHangfireExt
{
    public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(cfg =>
        {
            cfg.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(configuration["Database:HangfireConnectionString"]);
            });
        });
        
        return services;
    }
}