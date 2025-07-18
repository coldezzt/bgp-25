using System.Linq.Expressions;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

public interface IOperationRepository: IRepository<Operation>
{
    Task<Operation?> GetWithDetailsForProcessJobAsync(Expression<Func<Operation, bool>> filter, 
        CancellationToken cancellationToken = default);
}