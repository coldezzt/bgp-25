using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Data.Configurations;

public class OperationInstanceEntityConfiguration: IEntityTypeConfiguration<OperationInstance>
{
    public void Configure(EntityTypeBuilder<OperationInstance> builder)
    {
        builder.HasKey(oi => oi.Id);
        
        builder.Property(oi => oi.ScheduledAt)
            .IsRequired();
        
        builder.Property(oi => oi.Result)
            .HasMaxLength(200);
        
        builder.HasOne(oi => oi.Operation)
            .WithMany(o => o.History)
            .HasForeignKey(oi => oi.OperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}