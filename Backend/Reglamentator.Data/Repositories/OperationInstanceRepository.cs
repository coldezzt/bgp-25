using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

/// <summary>
/// Реализация репозитория <see cref="IOperationInstanceRepository"/> для работы с экземплярами операций.
/// </summary>
/// <remarks>
/// Использует <see cref="AppDbContext"/> - Контекст базы данных.
/// </remarks>
public class OperationInstanceRepository(
    AppDbContext appDbContext): Repository<OperationInstance>(appDbContext), IOperationInstanceRepository
{
    /// <inheritdoc/>
    public async Task<List<OperationInstance>> GetExecutedUserOperationsAsync(
        long telegramId, 
        CancellationToken cancellationToken = default) =>
        await AppDbContext.OperationInstances
            .Where(opi => opi.ExecutedAt != null)
            .Include(opi => opi.Operation)
            .Where(opi => opi.Operation.TelegramUserId == telegramId)
            .OrderBy(opi => opi.ExecutedAt)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<List<OperationInstance>> GetPlanedUserOperationsAsync(
        long telegramId, 
        TimeRange range, 
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var (startDate, endDate) = range switch
        {
            TimeRange.Day => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            TimeRange.Week => (now.Date.AddDays(-(int)now.DayOfWeek),
                now.Date.AddDays(-(int)now.DayOfWeek + 7).AddTicks(-1)),
            TimeRange.Month => (now.Date.AddDays(-now.Day + 1),
                now.Date.AddDays(-now.Day + 1).AddMonths(1).AddTicks(-1)),
            _ => throw new ArgumentOutOfRangeException(nameof(range), range.ToString())
        };
        
        var operations = await AppDbContext.OperationInstances
            .Where(opi => 
                opi.ExecutedAt == null &&
                opi.Operation.StartDate >= startDate &&
                opi.Operation.StartDate <= endDate)
            .Include(opi => opi.Operation)
            .Where(opi => opi.Operation.TelegramUserId == telegramId)
            .OrderBy(opi => opi.Operation.StartDate)
            .ToListAsync(cancellationToken);

        return operations;
    }
}