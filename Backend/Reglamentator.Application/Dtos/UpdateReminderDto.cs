using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Dtos;

/// <summary>
/// DTO для удаления напоминания.
/// </summary>
public class UpdateReminderDto
{
    public long Id { get; set; }
    public string MessageTemplate { get; set; } = null!;
    public TimeRange OffsetBeforeExecution { get; set; } 
}