using System.Linq.Expressions;

namespace Reglamentator.Domain.Interfaces;

public interface IRepository<TEntity>
{
    Task<TEntity?> GetEntityByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetEntitiesByFilterAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken = default);
    Task InsertEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
}