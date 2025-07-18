using AutoMapper;

namespace Reglamentator.WebAPI.Mapping;

public class ReminderMappingProfile: Profile
{
    public ReminderMappingProfile()
    {
        CreateMap<CreateReminderDto, Application.Dtos.CreateReminderDto>();
        
        CreateMap<UpdateReminderDto, Application.Dtos.UpdateReminderDto>();
        
        CreateMap<Domain.Entities.Reminder, ReminderDto>()
            .ForMember(dest => dest.OffsetMinutes, 
                opt => opt.MapFrom(src => (long)src.OffsetBeforeExecution.TotalMinutes));
        
        
    }
}