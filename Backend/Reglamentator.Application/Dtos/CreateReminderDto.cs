using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Dtos;

public class CreateReminderDto
{
    public string MessageTemplate { get; set; } = null!;
    public TimeRange OffsetBeforeExecution { get; set; } 
}