using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class ReminderRepository(AppDbContext appDbContext) : Repository<Reminder>(appDbContext), IReminderRepository
{
    public async Task<Reminder?> GetWithDetailsForProcessJobAsync(Expression<Func<Reminder, bool>> filter, CancellationToken cancellationToken = default) =>
        await AppDbContext.Reminders
            .Where(filter)
            .Include(r => r.Operation)
            .SingleOrDefaultAsync(cancellationToken);
}