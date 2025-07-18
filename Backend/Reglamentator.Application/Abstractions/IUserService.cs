using FluentResults;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

public interface IUserService
{
    Task<Result<TelegramUser>> CreateUserAsync(CreateUserDto userDto, 
        CancellationToken cancellationToken = default);
}