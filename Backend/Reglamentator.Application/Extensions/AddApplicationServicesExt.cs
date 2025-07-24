using Microsoft.Extensions.DependencyInjection;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Helpers;
using Reglamentator.Application.Managers;
using Reglamentator.Application.Services;

namespace Reglamentator.Application.Extensions;

/// <summary>
/// Методы расширения для регистрации сервисов приложения.
/// </summary>
public static class AddApplicationServicesExt
{
    /// <summary>
    /// Регистрирует все основные сервисы приложения.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов с зарегистрированными сервисами приложения.</returns>
    /// <remarks>
    /// Регистрирует:
    /// <list type="bullet">
    ///   <item>Сервисы операций, напоминаний и пользователей (scoped).</item>
    ///   <item>Вспомогательные сервисы через <see cref="AddApplicationHelpers"/>.</item>
    ///   <item>Менеджеры через <see cref="AddApplicationManagers"/>.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOperationService, OperationService>();
        services.AddScoped<IReminderService, ReminderService>();
        services.AddScoped<IUserService, UserService>();
        services.AddApplicationHelpers();
        services.AddApplicationManagers();
        
        return services;
    }

    /// <summary>
    /// Регистрирует менеджеры приложения (singleton).
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов с зарегистрированными менеджерами.</returns>
    /// <remarks>
    /// Включает:
    /// <list type="bullet">
    ///   <item>Менеджер потоков уведомлений.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddApplicationManagers(this IServiceCollection services)
    {
        services.AddSingleton<INotificationStreamManager, NotificationStreamManager>();
        
        return services;
    }

    /// <summary>
    /// Регистрирует вспомогательные сервисы приложения (singleton).
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов с зарегистрированными вспомогательными сервисами.</returns>
    /// <remarks>
    /// Включает:
    /// <list type="bullet">
    ///   <item>Вспомогательные сервисы для работы с Hangfire (напоминания и операции)</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddApplicationHelpers(this IServiceCollection services)
    {
        services.AddSingleton<IHangfireReminderJobHelper, HangfireReminderJobHelper>();
        services.AddSingleton<IHangfireOperationJobHelper, HangfireOperationJobHelper>();
        
        return services;
    }
}