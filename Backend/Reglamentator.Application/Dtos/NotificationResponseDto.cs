namespace Reglamentator.Application.Dtos;

/// <summary>
/// DTO для уведомления пользователя.
/// </summary>
public class NotificationResponseDto
{
    public long TelegramId { get; set; }
    public string Message { get; set; } = string.Empty;
}