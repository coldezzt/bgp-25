using AutoMapper;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.WebAPI.Mapping.Profiles;

public class TelegramUserMappingProfile: Profile
{
    public TelegramUserMappingProfile()
    {
        CreateMap<TelegramUserDto, CreateUserDto>()
            .ForMember(dest => dest.TelegramId, opt => opt.MapFrom(src => src.TelegramId));
        
        CreateMap<TelegramUser, TelegramUserDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TelegramId, opt => opt.MapFrom(src => src.TelegramId));
    }
}