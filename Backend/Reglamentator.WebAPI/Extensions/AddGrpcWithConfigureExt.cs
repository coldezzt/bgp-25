using Reglamentator.WebAPI.Validation;

namespace Reglamentator.WebAPI.Extensions;

/// <summary>
/// Методы расширения для настройки gRPC-сервисов.
/// </summary>
public static class AddGrpcWithConfigureExt
{
    /// <summary>
    /// Добавляет и настраивает gRPC сервисы в DI-контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов с зарегистрированными gRPC сервисами.</returns>
    /// <remarks>
    /// Основные настройки:
    /// <list type="bullet">
    ///   <item>Добавляет глобальный interceptor для валидации <see cref="ValidationInterceptor"/>.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddGrpcWithConfigure(this IServiceCollection services)
    {
        services.AddGrpc(options =>
        {
            options.Interceptors.Add<ValidationInterceptor>();
        });
        
        return services;
    }
}