namespace Reglamentator.Application.Dtos;

public class NotificationResponseDto
{
    public long TelegramId { get; set; }
    public string Message { get; set; } = string.Empty;
}