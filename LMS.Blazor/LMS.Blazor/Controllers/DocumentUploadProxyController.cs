using LMS.Blazor.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.Blazor.Controllers;

/// <summary>
/// Dedicated upload proxy that bypasses the generic ProxyController.
/// This is needed because UseAntiforgery() middleware consumes the multipart body
/// before the generic proxy can forward it.
/// </summary>
[ApiController]
[Route("api/upload")]
[Authorize]
public class DocumentUploadProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStorage _tokenStorage;

    public DocumentUploadProxyController(IHttpClientFactory httpClientFactory, ITokenStorage tokenStorage)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
    }

    [HttpPost("document")]
    [Authorize(Roles = "Teacher")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] int? courseId,
        [FromForm] int? moduleId,
        [FromForm] int? activityId)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { title = "Valideringsfel", detail = "Ingen fil har valts." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { title = "Unauthorized", detail = "Ogiltig eller saknad token." });

        var client = _httpClientFactory.CreateClient("LmsApiClient");

        var token = await _tokenStorage.GetAccessTokenAsync(userId);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();

        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "application/octet-stream";

        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", file.FileName);

        content.Add(new StringContent(name), "name");
        if (!string.IsNullOrWhiteSpace(description))
            content.Add(new StringContent(description), "description");
        if (courseId.HasValue)
            content.Add(new StringContent(courseId.Value.ToString()), "courseId");
        if (moduleId.HasValue)
            content.Add(new StringContent(moduleId.Value.ToString()), "moduleId");
        if (activityId.HasValue)
            content.Add(new StringContent(activityId.Value.ToString()), "activityId");

        var response = await client.PostAsync("api/documents/upload", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, responseBody);

        return Content(responseBody, "application/json");
    }

    [HttpPost("submission/{activityId:int}")]
    [Authorize(Roles = "Student")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> UploadSubmission(
        int activityId,
        [FromForm] IFormFile file,
        [FromForm] string? comment)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { title = "Valideringsfel", detail = "Ingen fil har valts." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized(new { title = "Unauthorized", detail = "Ogiltig eller saknad token." });

        var client = _httpClientFactory.CreateClient("LmsApiClient");

        var token = await _tokenStorage.GetAccessTokenAsync(userId);
        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();

        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "application/octet-stream";

        var streamContent = new StreamContent(file.OpenReadStream());
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "File", file.FileName);

        if (!string.IsNullOrWhiteSpace(comment))
            content.Add(new StringContent(comment), "Comment");

        var response = await client.PostAsync($"api/submissions/{activityId}", content);

        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, responseBody);

        return Content(responseBody, "application/json");
    }
}
