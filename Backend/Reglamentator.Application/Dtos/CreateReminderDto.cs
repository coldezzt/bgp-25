using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Dtos;

/// <summary>
/// DTO для создания нового напоминания.
/// </summary>
public class CreateReminderDto
{
    public string MessageTemplate { get; set; } = null!;
    public TimeRange OffsetBeforeExecution { get; set; } 
}