using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Data.Configurations;

public class TelegramUserEntityConfiguration: IEntityTypeConfiguration<TelegramUser>
{
    public void Configure(EntityTypeBuilder<TelegramUser> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.TelegramId)
            .IsRequired();
        
        builder.HasIndex(u => u.TelegramId)
            .IsUnique();

        builder.HasMany(u => u.Operations)
            .WithOne(o => o.TelegramUser)
            .HasForeignKey(o => o.TelegramUserId)
            .HasPrincipalKey(u => u.TelegramId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}