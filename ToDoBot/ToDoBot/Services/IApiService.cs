using ToDoBot.Models;

namespace ToDoBot.Services;

public interface IApiService
{
    Task<List<Operation>> GetOperationsAsync(long chatId);
    Task AddOperationAsync(Operation operation);
    Task DeleteOperationAsync(int id);

    Task<List<long>> GetAllUserIdsAsync();
    Task UpdateOperationAsync(Operation operation);

}