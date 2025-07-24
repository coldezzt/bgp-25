using FluentValidation;

namespace Reglamentator.WebAPI.Extensions;

/// <summary>
/// Методы расширения для регистрации валидаторов.
/// </summary>
public static class AddValidatorsExt
{
    /// <summary>
    /// Регистрирует все валидаторы из сборки, содержащей указанный тип.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Коллекция сервисов с зарегистрированными валидаторами.</returns>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description>Сканирует сборку, содержащую класс <see cref="Program"/>, для поиска валидаторов</description>.</item>
    ///   <item><description>Автоматически регистрирует все классы, реализующие <see cref="IValidator{T}"/></description>.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        
        return services;
    }
}