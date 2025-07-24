using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

/// <summary>
/// Реализация репозитория <see cref="IReminderRepository"/> для работы с напоминаниями.
/// </summary>
/// <remarks>
/// Использует <see cref="AppDbContext"/> - Контекст базы данных.
/// </remarks>
public class ReminderRepository(
    AppDbContext appDbContext): Repository<Reminder>(appDbContext), IReminderRepository
{
    /// <inheritdoc/>
    public async Task<Reminder?> GetWithDetailsForProcessJobAsync(
        Expression<Func<Reminder, bool>> filter, 
        CancellationToken cancellationToken = default) =>
        await AppDbContext.Reminders
            .Where(filter)
            .Include(r => r.Operation)
            .SingleOrDefaultAsync(cancellationToken);
}