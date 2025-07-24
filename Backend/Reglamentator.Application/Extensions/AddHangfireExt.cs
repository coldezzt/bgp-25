using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reglamentator.Application.Extensions;

/// <summary>
/// Методы расширения для настройки Hangfire.
/// </summary>
public static class AddHangfireExt
{
    /// <summary>
    /// Настраивает и добавляет сервисы Hangfire с PostgreSQL в качестве хранилища.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов с зарегистрированными сервисами Hangfire.</returns>
    /// <remarks>
    /// Выполняет следующие действия:
    /// <list type="number">
    ///   <item><description>Настраивает Hangfire для использования PostgreSQL в качестве хранилища</description></item>
    ///   <item><description>Указывает строку подключения из конфигурации (ключ Database:HangfireConnectionString)</description></item>
    ///   <item><description>Добавляет сервер Hangfire в коллекцию сервисов</description></item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(cfg =>
        {
            cfg.UsePostgreSqlStorage(options =>
            {
                options.UseNpgsqlConnection(configuration["Database:HangfireConnectionString"]);
            });
        });
        services.AddHangfireServer();
        
        return services;
    }
}