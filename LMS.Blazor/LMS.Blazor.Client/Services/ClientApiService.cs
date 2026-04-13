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

        if (HandleUnauthorized(response))
            return default;

        if (response.IsSuccessStatusCode)
        {
            return await JsonSerializer.DeserializeAsync<TResponse>(
                await response.Content.ReadAsStreamAsync(ct), _jsonOptions, ct);
        }

        var json = await response.Content.ReadAsStringAsync(ct);
        throw new Exception(ExtractErrorMessage(json));
    }

    public async Task<(bool Success, string? Error)> DeleteAsync(string endpoint, CancellationToken ct = default)
    {
        var response = await _httpClient.DeleteAsync($"api/proxy/{endpoint}", ct);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _navigationManager.NavigateTo("/Account/Login", forceLoad: true);
            return (false, null);
        }

        if (response.IsSuccessStatusCode)
            return (true, null);

        var errorBody = await response.Content.ReadAsStringAsync(ct);
        var errorMessage = errorBody.Trim('"');
        return (false, string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage);
    }
	public async Task<(bool Success, string? Error)> PatchAsync(string endpoint, CancellationToken ct = default)
	{
		var request = new HttpRequestMessage(HttpMethod.Patch, $"api/proxy/{endpoint}");

		var response = await _httpClient.SendAsync(request, ct);

		if (HandleUnauthorized(response))
			return (false, null);

		if (response.IsSuccessStatusCode)
			return (true, null);

		var errorBody = await response.Content.ReadAsStringAsync(ct);
		var errorMessage = ExtractErrorMessage(errorBody);

		return (false, string.IsNullOrWhiteSpace(errorMessage) ? null : errorMessage);
	}


	public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/proxy/{endpoint}", data, _jsonOptions, ct);

        if (HandleUnauthorized(response)) return default;

        if (!response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync(ct);
            throw new Exception(ExtractErrorMessage(json));
        }

        return await JsonSerializer.DeserializeAsync<TResponse>(
            await response.Content.ReadAsStreamAsync(ct), _jsonOptions, ct);
    }

    public async Task<TResponse?> PostMultipartAsync<TResponse>(string endpoint, MultipartFormDataContent content, CancellationToken ct = default)
    {
        // Calls the dedicated Blazor-server upload controller directly (NOT through the generic
        // api/proxy/... path), because UseAntiforgery() middleware consumes the multipart body
        // before the generic proxy can forward it.
        var response = await _httpClient.PostAsync(endpoint, content, ct);

        if (HandleUnauthorized(response)) return default;

        if (response.IsSuccessStatusCode)
        {
            return await JsonSerializer.DeserializeAsync<TResponse>(
                await response.Content.ReadAsStreamAsync(ct), _jsonOptions, ct);
        }

        var errorMessage = await response.Content.ReadAsStringAsync(ct);
        if (string.IsNullOrWhiteSpace(errorMessage))
            errorMessage = "Ett fel uppstod vid uppladdning.";

        throw new Exception(errorMessage.Trim('"'));
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

    /// <summary>
    /// Tries to extract a human-readable error message from a JSON error response.
    /// Falls back to the raw string if the response is not valid JSON.
    /// </summary>
    private static string ExtractErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("message", out var messageProp))
                return messageProp.GetString() ?? json;

            if (doc.RootElement.TryGetProperty("detail", out var detailProp))
                return detailProp.GetString() ?? json;

            if (doc.RootElement.TryGetProperty("title", out var titleProp))
                return titleProp.GetString() ?? json;
        }
        catch (JsonException)
        {
            // not valid JSON, fall back to raw text
        }

        return json;
    }
}
