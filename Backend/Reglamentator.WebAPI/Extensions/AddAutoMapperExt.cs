using Reglamentator.WebAPI.Mapping;

namespace Reglamentator.WebAPI.Extensions;

public static class AddAutoMapperExt
{
    public static IServiceCollection AddAutoMapperWithConfigure(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<TelegramUserMappingProfile>();
            cfg.AddProfile<ReminderMappingProfile>();
            cfg.AddProfile<OperationMappingProfile>();
            cfg.AddProfile<OperationInstanceMappingProfile>();
            cfg.AddProfile<TimeRangeMappingProfile>();
        });
        
        return services;
    }
}