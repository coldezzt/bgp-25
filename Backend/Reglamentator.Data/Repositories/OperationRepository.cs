using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class OperationRepository(
    AppDbContext appDbContext): Repository<Operation>(appDbContext), IOperationRepository
{
    public async Task<Operation?> GetWithDetailsForProcessJobAsync(
        Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default) =>
        await AppDbContext.Operations
            .Where(filter)
            .Include(op => op.NextOperationInstance)
            .Include(op => op.Reminders)
            .SingleOrDefaultAsync(cancellationToken);
}