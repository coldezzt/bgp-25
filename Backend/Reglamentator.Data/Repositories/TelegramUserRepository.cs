using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class TelegramUserRepository(AppDbContext appDbContext) : Repository<TelegramUser>(appDbContext), ITelegramUserRepository
{
}