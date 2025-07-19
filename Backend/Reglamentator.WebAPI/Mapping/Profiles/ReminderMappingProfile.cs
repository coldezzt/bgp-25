using AutoMapper;
using Reglamentator.Application.Extensions;

namespace Reglamentator.WebAPI.Mapping.Profiles;

public class ReminderMappingProfile: Profile
{
    public ReminderMappingProfile()
    {
        CreateMap<CreateReminderDto, Application.Dtos.CreateReminderDto>()
            .ForMember(dest => dest.OffsetBeforeExecution,
                opt => opt.MapFrom(src => src.OffsetBeforeExecution));
        
        CreateMap<UpdateReminderDto, Application.Dtos.UpdateReminderDto>()
            .ForMember(dest => dest.OffsetBeforeExecution,
                opt => opt.MapFrom(src => src.OffsetBeforeExecution));
        
        CreateMap<Domain.Entities.Reminder, ReminderDto>()
            .ForMember(dest => dest.OffsetBeforeExecution,
                opt => opt.MapFrom(src => src.OffsetBeforeExecution.ToTimeRange()));
    }
}