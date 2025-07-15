using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Domain.Entities;

public class TelegramUser : IEntity
{
    /// <inheritdoc/>
    public long Id { get; }

    /// <summary>
    /// Идентификатор пользователя в Telegram
    /// </summary>
    public long TelegramId { get; set; }
    
    public List<Operation> Operations { get; set; } = null!;
}