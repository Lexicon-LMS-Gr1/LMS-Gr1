using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.StudentDashboard;

namespace LMS.Blazor.Client.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default);
    Task<TResponse?> PostMultipartAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default);
    Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default);
    Task<(bool Success, string? Error)> DeleteAsync(string endpoint, CancellationToken ct = default);
	Task<(bool Success, string? Error)> PatchAsync(string endpoint, CancellationToken ct = default);
}