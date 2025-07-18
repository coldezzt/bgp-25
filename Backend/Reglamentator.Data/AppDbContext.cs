using Microsoft.EntityFrameworkCore;
using Reglamentator.Data.Configurations;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Operation> Operations => Set<Operation>();
    public DbSet<OperationInstance> OperationInstances => Set<OperationInstance>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<TelegramUser> TelegramUsers => Set<TelegramUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OperationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OperationInstanceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ReminderEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TelegramUserEntityConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}