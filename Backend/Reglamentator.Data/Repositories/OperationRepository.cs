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

    public async Task<Operation?> GetWithRemindersAsync(
        Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default) =>
            await AppDbContext.Operations
                .Where(filter)
                .Include(op => op.Reminders)
                .SingleOrDefaultAsync(cancellationToken);

    public override async Task InsertEntityAsync(
        Operation operation, 
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await AppDbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var isNextOperationInstanceNull = operation.NextOperationInstance == null;
            var operationHistory = operation.History;
            operation.NextOperationInstance = null;
            operation.History = [];
            await AppDbContext.SaveChangesAsync(cancellationToken);
            
            if(!isNextOperationInstanceNull)
                operation.NextOperationInstance = operationHistory.Last();
            
            operation.History = operationHistory;
            await AppDbContext.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}