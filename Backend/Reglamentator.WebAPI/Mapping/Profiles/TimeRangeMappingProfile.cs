using AutoMapper;

namespace Reglamentator.WebAPI.Mapping.Profiles;

public class TimeRangeMappingProfile: Profile
{
    public TimeRangeMappingProfile()
    {
        CreateMap<TimeRange, Domain.Entities.TimeRange>().ReverseMap();
    }
}