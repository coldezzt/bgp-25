using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class OperationRepository(AppDbContext appDbContext) : Repository<Operation>(appDbContext), IOperationRepository
{
}