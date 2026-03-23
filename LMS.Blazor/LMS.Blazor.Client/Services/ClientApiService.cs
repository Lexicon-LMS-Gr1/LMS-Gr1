using LMS.Shared.DTOs.Course;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Text.Json;

namespace LMS.Blazor.Client.Services;

public class ClientApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly NavigationManager _navigationManager;
    private readonly JsonSerializerOptions _jsonOptions;

    public ClientApiService(HttpClient httpClient, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _navigationManager = navigationManager;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default)
    {
        var response = await _httpClient.GetAsync($"api/proxy/{endpoint}", ct);

        if (HandleUnauthorized(response)) return default;

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<T>(
            await response.Content.ReadAsStreamAsync(ct), _jsonOptions, ct);
    }


    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/proxy/{endpoint}", data, _jsonOptions, ct);

        if (HandleUnauthorized(response)) return default;

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<TResponse>(
            await response.Content.ReadAsStreamAsync(ct), _jsonOptions, ct);
    }

    public async Task<bool> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/proxy/{endpoint}", ct);

        if (HandleUnauthorized(response)) return false;

        return response.IsSuccessStatusCode;
    }

    private bool HandleUnauthorized(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            _navigationManager.NavigateTo("/Account/Login", forceLoad: true);
            return true;
        }
        return false;
    }


    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/proxy/{endpoint}", data, _jsonOptions, ct);

        if (HandleUnauthorized(response)) return default;

        response.EnsureSuccessStatusCode();

        return await JsonSerializer.DeserializeAsync<TResponse>(
            await response.Content.ReadAsStreamAsync(ct), _jsonOptions, ct);
    }

    public async Task<IEnumerable<CourseDto>> GetCoursesAsync()
    {
        return await GetAsync<IEnumerable<CourseDto>>("api/course") ?? Enumerable.Empty<CourseDto>();
    }

    public async Task<CourseDto?> CreateCourseAsync(CourseCreateDto dto, CancellationToken ct = default)
    {
        return await PostAsync<CourseCreateDto, CourseDto>("api/course", dto, ct);
    }

}
