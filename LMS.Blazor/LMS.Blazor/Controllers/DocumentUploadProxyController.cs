using LMS.Blazor.Services;
using LMS.Shared.DTOs.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace LMS.Blazor.Controllers;

/// <summary>
/// Dedicated upload endpoint that avoids the generic proxy's body-forwarding issue.
/// The generic proxy uses StreamContent(Request.Body) but UseAntiforgery() middleware
/// reads and consumes the multipart body before the proxy action runs.
/// This controller uses [FromForm] to receive the file properly via model binding,
/// then re-constructs and forwards the multipart content to the real API.
/// </summary>
[Route("api/upload")]
[ApiController]
[Authorize]
[IgnoreAntiforgeryToken]
public class DocumentUploadProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ITokenStorage _tokenStorage;

    public DocumentUploadProxyController(
        IHttpClientFactory httpClientFactory,
        ITokenStorage tokenStorage)
    {
        _httpClientFactory = httpClientFactory;
        _tokenStorage = tokenStorage;
    }

    [HttpPost("document")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<IActionResult> UploadDocument(
        [FromForm] IFormFile file,
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] int? courseId,
        [FromForm] int? moduleId,
        [FromForm] int? activityId,
        CancellationToken ct)
    {
        // --- Auth ---
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var accessToken = await _tokenStorage.GetAccessTokenAsync(userId);
        if (string.IsNullOrWhiteSpace(accessToken))
            return Unauthorized("Unable to obtain valid access token");

        // --- Re-build multipart for the downstream API ---
        using var content = new MultipartFormDataContent();

        // Read the file into memory so it can be forwarded
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;

        var fileContent = new ByteArrayContent(ms.ToArray());
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        content.Add(fileContent, "file", file.FileName);

        content.Add(new StringContent(name),                       "name");
        content.Add(new StringContent(description ?? string.Empty), "description");

        if (courseId.HasValue)   content.Add(new StringContent(courseId.Value.ToString()),   "courseId");
        if (moduleId.HasValue)   content.Add(new StringContent(moduleId.Value.ToString()),   "moduleId");
        if (activityId.HasValue) content.Add(new StringContent(activityId.Value.ToString()), "activityId");

        // --- Forward to real API ---
        var client = _httpClientFactory.CreateClient("LmsApiClient");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        var apiResponse = await client.PostAsync("api/documents/upload", content, ct);

        var responseBody = await apiResponse.Content.ReadAsStringAsync(ct);

        return StatusCode((int)apiResponse.StatusCode,
            string.IsNullOrWhiteSpace(responseBody)
                ? null
                : JsonSerializer.Deserialize<object>(responseBody));
    }
}
