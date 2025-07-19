using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Reglamentator.Application.Extensions;

namespace Reglamentator.WebAPI.Mapping;

public class OperationMappingProfile: Profile
{
    public OperationMappingProfile()
    {
        CreateMap<CreateOperationDto, Application.Dtos.CreateOperationDto>()
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => src.StartDate.ToDateTime()))
            .ForMember(dest => dest.Reminders,
                opt => opt.MapFrom(src => src.Reminders))
            .ForMember(dest => dest.Cron,
            opt => opt.MapFrom(src => src.Cron));
        
        CreateMap<UpdateOperationDto, Application.Dtos.UpdateOperationDto>()
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => src.StartDate.ToDateTime()))
            .ForMember(dest => dest.Cron,
                opt => opt.MapFrom(src => src.Cron));
        
        CreateMap<Domain.Entities.Operation, OperationDto>()
            .ForMember(dest => dest.StartDate, 
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.StartDate)))
            .ForMember(dest => dest.Cron, 
                opt => opt.MapFrom(src => src.Cron.ToTimeRange()))
            .ForMember(dest => dest.Reminders, 
                opt => opt.MapFrom(src => src.Reminders));
    }
}