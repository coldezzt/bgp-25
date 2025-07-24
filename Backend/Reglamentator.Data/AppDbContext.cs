using Microsoft.EntityFrameworkCore;
using Reglamentator.Data.Configurations;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Data;

/// <summary>
/// Контекст базы данных приложения.
/// </summary>
/// <remarks>
/// Предоставляет доступ к таблицам базы данных и настраивает их конфигурацию.
/// Наследуется от <see cref="DbContext"/> Entity Framework Core.
/// </remarks>
/// <param name="options">Параметры конфигурации контекста.</param>
public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    /// <summary>
    /// Набор данных операций.
    /// </summary>
    public DbSet<Operation> Operations => Set<Operation>();
    
    /// <summary>
    /// Набор данных экземпляров операций.
    /// </summary>
    public DbSet<OperationInstance> OperationInstances => Set<OperationInstance>();
    
    /// <summary>
    /// Набор данных напоминаний.
    /// </summary>
    public DbSet<Reminder> Reminders => Set<Reminder>();
    
    /// <summary>
    /// Набор данных пользователей Telegram.
    /// </summary>
    public DbSet<TelegramUser> TelegramUsers => Set<TelegramUser>();

    /// <summary>
    /// Настраивает модель базы данных при создании.
    /// </summary>
    /// <param name="modelBuilder">Построитель модели Entity Framework.</param>
    /// <remarks>
    /// Применяет конфигурации сущностей:
    /// <list type="bullet">
    ///   <item><description><see cref="OperationEntityConfiguration"/></description></item>
    ///   <item><description><see cref="OperationInstanceEntityConfiguration"/></description></item>
    ///   <item><description><see cref="ReminderEntityConfiguration"/></description></item>
    ///   <item><description><see cref="TelegramUserEntityConfiguration"/></description></item>
    /// </list>
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OperationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OperationInstanceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ReminderEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TelegramUserEntityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}