using System.Linq.Expressions;

namespace Reglamentator.Domain.Interfaces;

/// <summary>
/// Базовый интерфейс репозитория для работы с сущностями.
/// </summary>
/// <typeparam name="TEntity">Тип сущности, должен реализовывать <see cref="IEntity"/>.</typeparam>
public interface IRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Проверяет существование сущности по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор сущности.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>True, если сущность существует, иначе False</returns>
    Task<bool> IsExistAsync(long id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает сущность по условию фильтрации.
    /// </summary>
    /// <param name="filter">Лямбда-выражение для фильтрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Найденная сущность или null.</returns>
    Task<TEntity?> GetEntityByFilterAsync(Expression<Func<TEntity, bool>> filter, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Получает список сущностей по условию фильтрации.
    /// </summary>
    /// <param name="filter">Лямбда-выражение для фильтрации.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Список найденных сущностей.</returns>
    Task<List<TEntity>> GetEntitiesByFilterAsync(Expression<Func<TEntity, bool>> filter, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Добавляет новую сущность в базу данных.
    /// </summary>
    /// <param name="entity">Сущность для добавления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task InsertEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Обновляет существующую сущность в базе данных.
    /// </summary>
    /// <param name="entity">Сущность для обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task UpdateEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Удаляет сущность из базы данных.
    /// </summary>
    /// <param name="entity">Сущность для удаления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    Task DeleteEntityAsync(TEntity entity, CancellationToken cancellationToken = default);
}