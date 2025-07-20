using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Data.Configurations;

public class OperationEntityConfiguration: IEntityTypeConfiguration<Operation>
{
    public void Configure(EntityTypeBuilder<Operation> builder)
    {
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Theme).IsRequired();
        builder.Property(o => o.Description).IsRequired();
        builder.Property(o => o.StartDate).IsRequired();
        
        builder.HasOne(o => o.TelegramUser)
            .WithMany(u => u.Operations)
            .HasForeignKey(o => o.TelegramUserId)
            .HasPrincipalKey(u => u.TelegramId);

        builder.HasOne(x => x.NextOperationInstance)
            .WithOne()
            .HasForeignKey<Operation>(x => x.NextOperationInstanceId);
    }
}