using FluentResults;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Services;

/// <summary>
/// Реализация <see cref="IUserService"/> для работы с пользователями Telegram.
/// </summary>
/// <remarks>
/// Использует <see cref="ITelegramUserRepository"/> для доступа к данным <see cref="TelegramUser"/>.
/// </remarks>
public class UserService(
    ITelegramUserRepository userRepository
    ): IUserService
{
    
    /// <inheritdoc/>
    public async Task<Result<TelegramUser>> CreateUserAsync(
        CreateUserDto userDto, 
        CancellationToken cancellationToken = default)
    {
        var telegramUser = await userRepository.GetEntityByFilterAsync(
            tu => tu.TelegramId == userDto.TelegramId, cancellationToken);

        if (telegramUser != null)
            return Result.Ok(telegramUser);
            //return Result.Fail(new CreateUserError(CreateUserError.TelegramUserAlreadyExist));
        
        var user = CreateNewUser(userDto);
        await userRepository.InsertEntityAsync(user, cancellationToken);
        
        return Result.Ok(user);
    }

    /// <summary>
    /// Создает новый экземпляр TelegramUser из DTO.
    /// </summary>
    /// <param name="userDto">Данные пользователя.</param>
    /// <returns>Новый пользователь.</returns>
    private TelegramUser CreateNewUser(CreateUserDto userDto) =>
        new()
        {
            TelegramId = userDto.TelegramId
        };
}