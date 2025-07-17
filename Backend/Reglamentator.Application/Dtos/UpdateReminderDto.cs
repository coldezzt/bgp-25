namespace Reglamentator.Application.Dtos;

public class UpdateReminderDto
{
    public long Id { get; set; }
    public string MessageTemplate { get; set; } = null!;
    public long OffsetMinutes { get; set; } 
}