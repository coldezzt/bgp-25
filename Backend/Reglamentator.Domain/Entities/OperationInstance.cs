using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Domain.Entities;

/// <summary>
/// Исполненная задача
/// </summary>
public class OperationInstance : IEntity
{
    /// <inheritdoc/>
    public long Id { get; set; }
    
    /// <summary>
    /// Когда операция была запланирована
    /// </summary>
    public DateTime ScheduledAt { get; set; }
    
    public long OperationId { get; set; }
    public Operation Operation { get; set; } = null!;
    
    /// <summary>
    /// Результат операции
    /// </summary>
    public string? Result { get; set; }
    
    /// <summary>
    /// Фактическая дата выполнения операции
    /// </summary>
    public DateTime? ExecutedAt { get; set; }
}
