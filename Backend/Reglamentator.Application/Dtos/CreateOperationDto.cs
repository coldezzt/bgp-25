namespace Reglamentator.Application.Dtos;

public class CreateOperationDto
{
    public string Theme { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public string? Cron { get; set; }
    public List<CreateReminderDto> Reminders { get; set; } = null!;
}