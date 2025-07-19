using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Dtos;

public class UpdateOperationDto
{
    public long Id { get; set; }
    public string Theme { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public TimeRange Cron { get; set; }
}