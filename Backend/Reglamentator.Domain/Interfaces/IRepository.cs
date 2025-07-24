using System.Linq.Expressions;

namespace Reglamentator.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task<bool> IsExistAsync(long id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetEntityByFilterAsync(Expression<Func<TEntity, bool>> filter, 
        CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetEntitiesByFilterAsync(Expression<Func<TEntity, bool>> filter, 
        CancellationToken cancellationToken = default);
    Task InsertEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
}