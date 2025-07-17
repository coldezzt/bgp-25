using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public abstract class Repository<T>(AppDbContext appDbContext): IRepository<T> where T : class, IEntity
{
    protected readonly AppDbContext AppDbContext = appDbContext;
        
    public async Task<bool> IsExistAsync(long id, CancellationToken cancellationToken = default) =>
        await AppDbContext.Set<T>()
            .AnyAsync(e => e.Id == id, cancellationToken);
    
    public virtual async Task<List<T>> GetEntitiesByFilterAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
        await AppDbContext.Set<T>()
            .Where(filter)
            .ToListAsync(cancellationToken);

    public virtual async Task<T?> GetEntityByFilterAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) => 
        await AppDbContext.Set<T>()
            .SingleOrDefaultAsync(filter, cancellationToken);

    public virtual async Task InsertEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        await AppDbContext.AddAsync(entity, cancellationToken);
        await AppDbContext.SaveChangesAsync(cancellationToken);
    }

    public virtual async Task UpdateEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        AppDbContext.Update(entity);
        await AppDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        AppDbContext.Remove(entity);
        await AppDbContext.SaveChangesAsync(cancellationToken);
    }
}