using FluentResults;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Services;

public class UserService(
    ITelegramUserRepository userRepository
    ): IUserService
{
    public async Task<Result<TelegramUser>> CreateUserAsync(
        CreateUserDto userDto, 
        CancellationToken cancellationToken = default)
    {
        var telegramUser = await userRepository.GetEntityByFilterAsync(
            tu => tu.TelegramId == userDto.TelegramId, cancellationToken);

        if (telegramUser != null)
            return Result.Fail(new CreateUserError(CreateUserError.TelegramUserAlreadyExist));
        
        var user = CreateNewUser(userDto);
        await userRepository.InsertEntityAsync(user, cancellationToken);
        
        return Result.Ok(user);
    }

    private TelegramUser CreateNewUser(CreateUserDto userDto) =>
        new()
        {
            TelegramId = userDto.TelegramId
        };
}