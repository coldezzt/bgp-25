namespace Reglamentator.Application.Dtos;

public class CreateReminderDto
{
    public string MessageTemplate { get; set; } = null!;
    public long OffsetMinutes { get; set; } 
}