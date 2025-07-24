using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

/// <summary>
/// Реализация репозитория <see cref="IOperationRepository"/> для работы с операциями.
/// </summary>
/// <remarks>
/// Использует <see cref="AppDbContext"/> - Контекст базы данных.
/// </remarks>
public class OperationRepository(
    AppDbContext appDbContext): Repository<Operation>(appDbContext), IOperationRepository
{
    /// <inheritdoc/>
    public async Task<Operation?> GetWithDetailsForProcessJobAsync(
        Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default) =>
        await AppDbContext.Operations
            .Where(filter)
            .Include(op => op.NextOperationInstance)
            .Include(op => op.Reminders)
            .SingleOrDefaultAsync(cancellationToken);

    /// <inheritdoc/>
    public async Task<Operation?> GetWithRemindersAsync(
        Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default) =>
            await AppDbContext.Operations
                .Where(filter)
                .Include(op => op.Reminders)
                .SingleOrDefaultAsync(cancellationToken);
    
    /// <summary>
    /// Добавляет новую операцию в базу данных.
    /// </summary>
    /// <param name="operation">Операция для добавления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <remarks>
    /// <para>
    /// Особенности реализации:
    /// </para>
    /// <list type="number">
    ///   <item>Выполняется в транзакции.</item>
    ///   <item>Временно очищает NextOperationInstance и History перед сохранением.</item>
    ///   <item>Восстанавливает связанные данные после сохранения основной операции.</item>
    /// </list>
    /// </remarks>
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
            await AppDbContext.Operations.AddAsync(operation, cancellationToken);
            await AppDbContext.SaveChangesAsync(cancellationToken);
            
            if(!isNextOperationInstanceNull)
                operation.NextOperationInstance = operationHistory.Last();
            
            operation.History = operationHistory;
            AppDbContext.Update(operation);
            await AppDbContext.SaveChangesAsync(cancellationToken);
            
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            // ignored
        }
    }
}