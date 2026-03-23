using LMS.Blazor.Client.Services;

namespace LMS.Blazor.Services;

public class ServerNoOpApiService(ILogger<ServerNoOpApiService> logger) : IApiService
{
    private readonly ILogger<ServerNoOpApiService> _logger = logger;

    public Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        _logger.LogWarning("ServerNoOpApiService.GetAsync called for: {Endpoint}", endpoint);
        return Task.FromResult<T?>(default);
    }

    public Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default)
    {
        _logger.LogWarning("ServerNoOpApiService.PostAsync called for: {Endpoint}", endpoint);
        return Task.FromResult<TResponse?>(default);
    }

    public Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default)
    {
        _logger.LogWarning("ServerNoOpApiService.PutAsync called for: {Endpoint}", endpoint);
        return Task.FromResult<TResponse?>(default);
    }

    public Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        _logger.LogWarning("ServerNoOpApiService.DeleteAsync called for: {Endpoint}", endpoint);
        return Task.FromResult(false);
    }
}
