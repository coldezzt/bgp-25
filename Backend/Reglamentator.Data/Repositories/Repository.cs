using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

/// <summary>
/// Базовая реализация репозитория для работы с сущностями
/// </summary>
/// <typeparam name="T">Тип сущности, должен реализовывать <see cref="IEntity"/>.</typeparam>
/// <remarks>
/// Использует <see cref="AppDbContext"/> - Контекст базы данных.
/// </remarks>
public abstract class Repository<T>(AppDbContext appDbContext): IRepository<T> where T : class, IEntity
{
    /// <summary>
    /// Защищенное поле для доступа к контексту базы данных.
    /// </summary>
    protected readonly AppDbContext AppDbContext = appDbContext;
    
    /// <inheritdoc/>
    public virtual async Task<bool> IsExistAsync(long id, CancellationToken cancellationToken = default) =>
        await AppDbContext.Set<T>()
            .AnyAsync(e => e.Id == id, cancellationToken);
    
    /// <inheritdoc/>
    public virtual async Task<List<T>> GetEntitiesByFilterAsync(
        Expression<Func<T, bool>> filter, 
        CancellationToken cancellationToken = default) =>
        await AppDbContext.Set<T>()
            .Where(filter)
            .ToListAsync(cancellationToken);

    /// <inheritdoc/>
    public virtual async Task<T?> GetEntityByFilterAsync(
        Expression<Func<T, bool>> filter, 
        CancellationToken cancellationToken = default) => 
        await AppDbContext.Set<T>()
            .SingleOrDefaultAsync(filter, cancellationToken);

    /// <inheritdoc/>
    public virtual async Task InsertEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        await AppDbContext.AddAsync(entity, cancellationToken);
        await AppDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task UpdateEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        AppDbContext.Update(entity);
        await AppDbContext.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public virtual async Task DeleteEntityAsync(T entity, CancellationToken cancellationToken = default)
    {
        AppDbContext.Remove(entity);
        await AppDbContext.SaveChangesAsync(cancellationToken);
    }
}