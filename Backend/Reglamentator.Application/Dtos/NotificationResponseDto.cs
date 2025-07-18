namespace Reglamentator.Application.Dtos;

public class NotificationResponseDto
{
    public int TelegramId { get; set; }
    public string Message { get; set; } = string.Empty;
}