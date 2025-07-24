using System.Reflection;

namespace Reglamentator.WebAPI.Extensions;

/// <summary>
/// Методы расширения для настройки AutoMapper.
/// </summary>
public static class AddAutoMapperExt
{
    /// <summary>
    /// Добавляет и настраивает AutoMapper в DI-контейнере.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов с зарегистрированным AutoMapper.</returns>
    public static IServiceCollection AddAutoMapperWithConfigure(this IServiceCollection services)
    {
        services.AddAutoMapper(_ => {}, Assembly.GetExecutingAssembly());
        
        return services;
    }
}