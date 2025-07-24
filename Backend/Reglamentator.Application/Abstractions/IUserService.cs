using FluentResults;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

/// <summary>
/// Предоставляет функциональность для работы с пользователями.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Создает нового пользователя Telegram или возвращает существующего, если он уже зарегистрирован.
    /// </summary>
    /// <param name="userDto">DTO с данными пользователя.</param>
    /// <param name="cancellationToken">Токен отмены (опционально).</param>
    /// <returns>
    /// Результат, содержащий либо созданного/существующего <see cref="TelegramUser"/>,
    /// либо ошибку <see cref="Error"/> в случае неудачи.
    /// </returns>
    Task<Result<TelegramUser>> CreateUserAsync(CreateUserDto userDto, 
        CancellationToken cancellationToken = default);
}