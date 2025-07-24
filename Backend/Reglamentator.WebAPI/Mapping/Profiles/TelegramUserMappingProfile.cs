using AutoMapper;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.WebAPI.Mapping.Profiles;

public class TelegramUserMappingProfile: Profile
{
    public TelegramUserMappingProfile()
    {
        CreateMap<TelegramUserDto, CreateUserDto>();

        CreateMap<TelegramUser, TelegramUserDto>();
    }
}