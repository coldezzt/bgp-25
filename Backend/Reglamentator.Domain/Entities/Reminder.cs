using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Domain.Entities;

/// <summary>
/// Напоминание
/// </summary>
public class Reminder : IEntity
{
    /// <inheritdoc/>
    public long Id { get; set; }

    /// <summary>
    /// Формат уведомления
    /// </summary>
    public string MessageTemplate { get; set; } = null!;
    /// <summary>
    /// За сколько времени "до" должно быть отправлено уведомление
    /// </summary>
    public TimeSpan OffsetBeforeExecution { get; set; }

    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;
}
