using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Data.Repositories;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Extensions;

/// <summary>
/// Методы расширения для настройки и регистрации контекста базы данных и репозиториев.
/// </summary>
public static class AddDbContextExt
{
    /// <summary>
    /// Регистрирует контекст базы данных и связанные репозитории в DI-контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <param name="configuration">Конфигурация приложения.</param>
    /// <returns>Коллекция сервисов с зарегистрированными зависимостями.</returns>
    /// <remarks>
    /// Выполняет следующие действия:
    /// <list type="number">
    ///   <item>Регистрирует все репозитории (scoped).</item>
    ///   <item>Настраивает подключение к PostgreSQL и добавляет <see cref="AppDbContext"/></item>
    /// </list>
    /// </remarks>
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