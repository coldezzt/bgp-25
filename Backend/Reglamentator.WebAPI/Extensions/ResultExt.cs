using AutoMapper;
using FluentResults;

namespace Reglamentator.WebAPI.Extensions;

/// <summary>
/// Методы расширения для преобразования объектов <see cref="Result{T}"/> в различные типы ответов.
/// </summary>
public static class ResultExt
{
    /// <summary>
    /// Преобразует <see cref="Result{T}"/> в <see cref="StatusResponse"/>.
    /// </summary>
    /// <typeparam name="T">Тип значения результата.</typeparam>
    /// <param name="result">Результат для преобразования.</param>
    /// <returns>
    /// <see cref="StatusResponse"/> содержащий:
    /// <list type="bullet">
    ///   <item>Флаг IsSuccess из результата.</item>
    ///   <item>Сообщение "Success" при успехе, иначе первое сообщение об ошибке.</item>
    /// </list>
    /// </returns>
    public static StatusResponse ToStatusResponse<T>(this Result<T> result)
        => new()
        {
            IsSuccess = result.IsSuccess,
            StatusMessage = result.IsSuccess ? "Success" : result.Errors[0].Message
        };

    /// <summary>
    /// Преобразует успешный результат в DTO ответа, или возвращает новый экземпляр при ошибке.
    /// </summary>
    /// <typeparam name="T">Исходный тип.</typeparam>
    /// <typeparam name="TV">Тип назначения.</typeparam>
    /// <param name="result">Результат для преобразования.</param>
    /// <param name="mapper">Экземпляр AutoMapper для маппинга.</param>
    /// <returns>
    /// Преобразованный DTO ответа при успехе, иначе новый экземпляр типа ответа.
    /// </returns>
    /// <remarks>
    /// Оба типа должны быть классами. Тип назначения должен иметь конструктор без параметров.
    /// </remarks>
    public static TV ToResponseData<T, TV>(this Result<T> result, IMapper mapper) 
        where T : class
        where TV : class, new()
        => result.IsSuccess ? mapper.Map<TV>(result.Value) : new TV();
    
    /// <summary>
    /// Преобразует успешный результат со списком в список DTO ответов, или возвращает пустой список при ошибке.
    /// </summary>
    /// <typeparam name="T">Исходный тип.</typeparam>
    /// <typeparam name="TV">Тип назначения.</typeparam>
    /// <param name="result">Результат для преобразования.</param>
    /// <param name="mapper">Экземпляр AutoMapper для маппинга.</param>
    /// <returns>
    /// Преобразованный список DTO ответов при успехе, иначе пустой список.
    /// </returns>
    /// <remarks>
    /// Оба типа должны быть классами. Тип назначения должен быть классом (не требует конструктора без параметров).
    /// </remarks>
    public static List<TV> ToResponseData<T, TV>(this Result<List<T>> result, IMapper mapper) 
        where T : class
        where TV : class
        => result.IsSuccess ? mapper.Map<List<TV>>(result.Value) : [];
}