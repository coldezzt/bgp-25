using AutoMapper;
using Google.Protobuf.WellKnownTypes;

namespace Reglamentator.WebAPI.Mapping.Profiles;

public class OperationInstanceMappingProfile: Profile
{
    public OperationInstanceMappingProfile()
    {
        CreateMap<Domain.Entities.OperationInstance, OperationInstanceDto>()
            .ForMember(dest => dest.ScheduledAt, 
                opt => opt.MapFrom(src => Timestamp.FromDateTime(src.ScheduledAt)))
            .ForMember(dest => dest.ExecutedAt, 
                opt => opt.MapFrom(src => 
                    src.ExecutedAt.HasValue 
                        ? Timestamp.FromDateTime(src.ExecutedAt.Value) 
                        : null))
            .ForMember(dest => dest.Result, 
                opt => opt.MapFrom(src => 
                    !string.IsNullOrEmpty(src.Result) 
                        ? new StringValue { Value = src.Result } 
                        : null))
            .ForMember(dest => dest.Operation, 
                opt => opt.MapFrom(src => src.Operation));
    }
}