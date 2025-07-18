using AutoMapper;
using Grpc.Core;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;
using Reglamentator.WebAPI.Extensions;

namespace Reglamentator.WebAPI.Services;

public class UserGrpcService(
    IUserService userService,
    IMapper mapper
    ): User.UserBase
{
    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var result = await userService.CreateUserAsync(new CreateUserDto{TelegramId = request.TelegramId});

        return new CreateUserResponse
        {
            Status = result.ToStatusResponse(),
            User = result.ToResponseData<TelegramUser, TelegramUserDto>(mapper)
        };
    }
}