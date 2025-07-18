using AutoMapper;

namespace Reglamentator.WebAPI.Mapping;

public class TimeRangeMappingProfile: Profile
{
    public TimeRangeMappingProfile()
    {
        CreateMap<TimeRange, Domain.Entities.TimeRange>().ReverseMap();
    }
}