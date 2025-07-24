using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class TelegramUserRepository(AppDbContext appDbContext) : Repository<TelegramUser>(appDbContext), ITelegramUserRepository
{
    public override async Task<bool> IsExistAsync(long id, CancellationToken cancellationToken = default) =>
        await AppDbContext.TelegramUsers
            .AnyAsync(e => e.TelegramId == id, cancellationToken);
}