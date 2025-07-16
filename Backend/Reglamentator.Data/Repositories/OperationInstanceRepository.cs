using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

public class OperationInstanceRepository(AppDbContext appDbContext) : Repository<OperationInstance>(appDbContext), IOperationInstanceRepository
{
}