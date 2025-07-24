namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет метод для асинхронной записи данных в поток.
/// </summary>
/// <typeparam name="T">Тип данных для записи в поток.</typeparam>
public interface IStreamWriter<in T>
{
    /// <summary>
    /// Асинхронно записывает сообщение в поток.
    /// </summary>
    /// <param name="message">Сообщение для записи.</param>
    /// <returns>Задача, представляющая асинхронную операцию записи.</returns>
    Task WriteAsync(T message);
}