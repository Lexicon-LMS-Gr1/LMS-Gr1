using System.Security.Claims;
using Domain.Models.Exceptions;
using LMS.Presentation.Models;
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
    /// Provide exactly one of courseId, moduleId, or activityId.
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "Teacher")]
    [RequestSizeLimit(52_428_800)]
    public async Task<ActionResult<DocumentDto>> Upload([FromForm] DocumentUploadForm form)
    {
        if (form.File == null || form.File.Length == 0)
            throw new BadRequestException("No file was provided.", "Valideringsfel");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new TokenValidationException();

        var dto = new DocumentCreateDto
        {
            Name = form.Name,
            Description = form.Description,
            CourseId = form.CourseId,
            ModuleId = form.ModuleId,
            ActivityId = form.ActivityId
        };

        using var stream = form.File.OpenReadStream();
        var result = await _serviceManager.DocumentService.UploadAsync(
            dto, stream, form.File.FileName, form.File.ContentType, form.File.Length, userId);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Get document metadata by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DocumentDto>> GetById(int id)
    {
        var document = await _serviceManager.DocumentService.GetByIdAsync(id);
        return Ok(document);
    }

    /// <summary>
    /// Download a document file.
    /// </summary>
    [HttpGet("{id:int}/download")]
    public async Task<IActionResult> Download(int id)
    {
        var result = await _serviceManager.DocumentService.DownloadAsync(id);

        var (fileStream, contentType, fileName) = result;
        return File(fileStream, contentType, fileName);
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

        throw new BadRequestException("Ange en av följande: courseId, moduleId, eller activityId.", "Valideringsfel");
    }

    /// <summary>
    /// Delete a document (file + DB record). Teacher only.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        await _serviceManager.DocumentService.DeleteAsync(id);

        return NoContent();
    }
}