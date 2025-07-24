using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Domain.Entities;

public class Operation : IEntity
{
    /// <inheritdoc/>
    public long Id { get; set; }

    public string Theme { get; set; } = null!;
    public string Description { get; set; } = null!;

    /// <summary>
    /// Начало действия операции
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// Повторы задачи в формате Cron. <br/>
    /// null - для одноразовой задачи.
    /// </summary>
    public string? Cron { get; set; }

    public long TelegramUserId { get; set; }
    public TelegramUser TelegramUser { get; set; } = null!;

    public List<Reminder> Reminders { get; set; } = null!;
    public List<OperationInstance> History { get; set; } = null!;
}