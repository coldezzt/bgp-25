using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class ReminderRepository(AppDbContext appDbContext) : Repository<Reminder>(appDbContext), IReminderRepository
{
}