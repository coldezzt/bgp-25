using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Dtos;

/// <summary>
/// DTO для создания новой операции.
/// </summary>
public class CreateOperationDto
{
    public string Theme { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public TimeRange Cron { get; set; }
}