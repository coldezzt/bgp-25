using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace Reglamentator.WebAPI.Mapping;

public class OperationMappingProfile: Profile
{
    public OperationMappingProfile()
    {
        CreateMap<CreateOperationDto, Application.Dtos.CreateOperationDto>()
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => src.StartDate.ToDateTime()))
            .ForMember(dest => dest.Reminders,
                opt => opt.MapFrom(src => src.Reminders));

        CreateMap<UpdateOperationDto, Application.Dtos.UpdateOperationDto>()
            .ForMember(dest => dest.StartDate,
                opt => opt.MapFrom(src => src.StartDate.ToDateTime()));
        
        CreateMap<Domain.Entities.Operation, OperationDto>()
            .ForMember(dest => dest.StartDate, 
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.StartDate)))
            .ForMember(dest => dest.Cron, 
                opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.Cron) 
                        ? new StringValue { Value = src.Cron } 
                        : null))
            .ForMember(dest => dest.Reminders, 
                opt => opt.MapFrom(src => src.Reminders));
    }
}