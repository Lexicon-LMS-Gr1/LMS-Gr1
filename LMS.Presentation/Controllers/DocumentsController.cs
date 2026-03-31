using System.Security.Claims;
using LMS.Shared.DTOs.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public DocumentsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    /// <summary>
    /// Upload a document to a Course, Module, or Activity.
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Teacher")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(52_428_800)] // 50 MB
    public async Task<ActionResult<DocumentDto>> Upload(
        [FromForm] IFormFile file,
        [FromForm] string name,
        [FromForm] string? description,
        [FromForm] int? courseId,
        [FromForm] int? moduleId,
        [FromForm] int? activityId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file was provided.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dto = new DocumentCreateDto
        {
            Name = name,
            Description = description,
            CourseId = courseId,
            ModuleId = moduleId,
            ActivityId = activityId
        };

        try
        {
            using var stream = file.OpenReadStream();
            var result = await _serviceManager.DocumentService.UploadAsync(
                dto, stream, file.FileName, file.ContentType, file.Length, userId);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get document metadata by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetById(int id)
    {
        var document = await _serviceManager.DocumentService.GetByIdAsync(id);
        if (document == null)
            return NotFound($"Document with ID {id} not found.");

        return Ok(document);
    }

    /// <summary>
    /// Download a document file.
    /// </summary>
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        try
        {
            var result = await _serviceManager.DocumentService.DownloadAsync(id);
            if (result == null)
                return NotFound($"Document with ID {id} not found.");

            var (fileStream, contentType, fileName) = result.Value;
            return File(fileStream, contentType, fileName);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// List documents by parent entity. Provide exactly one query parameter.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByParent(
        [FromQuery] int? courseId,
        [FromQuery] int? moduleId,
        [FromQuery] int? activityId)
    {
        if (courseId.HasValue)
            return Ok(await _serviceManager.DocumentService.GetByCourseIdAsync(courseId.Value));

        if (moduleId.HasValue)
            return Ok(await _serviceManager.DocumentService.GetByModuleIdAsync(moduleId.Value));

        if (activityId.HasValue)
            return Ok(await _serviceManager.DocumentService.GetByActivityIdAsync(activityId.Value));

        return BadRequest("Provide one of: courseId, moduleId, or activityId.");
    }

    /// <summary>
    /// Delete a document (file + DB record). Teacher only.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _serviceManager.DocumentService.DeleteAsync(id);
        if (!result)
            return NotFound($"Document with ID {id} not found.");

        return NoContent();
    }
}
