using System.Linq.Expressions;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

public interface IReminderRepository: IRepository<Reminder>
{
    Task<Reminder?> GetWithDetailsForProcessJobAsync(Expression<Func<Reminder, bool>> filter, 
        CancellationToken cancellationToken = default);
}