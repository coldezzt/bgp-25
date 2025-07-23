using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Data.Configurations;

public class ReminderEntityConfiguration: IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.HasKey(r => r.Id);
        
        builder.Property(r => r.MessageTemplate)
            .IsRequired()
            .HasMaxLength(1000);
        
        builder.Property(r => r.OffsetBeforeExecution)
            .IsRequired();
        
        builder.HasOne(r => r.Operation)
            .WithMany(o => o.Reminders)
            .HasForeignKey(r => r.OperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}