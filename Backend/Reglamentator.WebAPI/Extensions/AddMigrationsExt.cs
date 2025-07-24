using Microsoft.EntityFrameworkCore;
using Reglamentator.Data;

namespace Reglamentator.WebAPI.Extensions;

public static class AddMigrationsExt
{
    public static async Task AddMigrations(this WebApplication application)
    {
        using var scope = application.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        await context.Database.MigrateAsync();
    }
}