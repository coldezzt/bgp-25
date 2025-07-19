using System.Net.Http.Json;
using ToDoBot.Models;

namespace ToDoBot.Services;

public class ApiService : IApiService
{
    private readonly HttpClient _client;

    public ApiService(string baseUrl)
    {
        _client = new HttpClient { BaseAddress = new Uri(baseUrl) };
    }

    public async Task<List<Operation>> GetOperationsAsync(long chatId)
    {
        return await _client.GetFromJsonAsync<List<Operation>>($"operations?chatId={chatId}")
               ?? new List<Operation>();
    }

    public async Task AddOperationAsync(Operation operation)
    {
        var response = await _client.PostAsJsonAsync("operations", operation);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteOperationAsync(int id)
    {
        var response = await _client.DeleteAsync($"operations/{id}");
        response.EnsureSuccessStatusCode();
    }


    public async Task UpdateOperationAsync(Operation operation)
    {
        var response = await _client.PutAsJsonAsync($"operations/{operation.Id}", operation);
        response.EnsureSuccessStatusCode();
    }


    public async Task<List<long>> GetAllUserIdsAsync()
    {
        return await _client.GetFromJsonAsync<List<long>>("operations/users") ?? new List<long>();
    }

}