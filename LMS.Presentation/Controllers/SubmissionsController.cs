using Domain.Contracts.Services;
using Domain.Models.Exceptions;
using LMS.Presentation.Models;
using LMS.Shared.DTOs.Feedback;
using LMS.Shared.DTOs.Submission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/submissions")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly IServiceManager _serviceManager;
    private readonly IFileStorageService _fileStorage;

    public SubmissionsController(IServiceManager serviceManager, IFileStorageService fileStorage)
    {
        _serviceManager = serviceManager;
        _fileStorage = fileStorage;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new TokenValidationException();

        var isTeacher = User.IsInRole("Teacher");

        var submission = await _serviceManager.SubmissionService.GetSubmissionByIdAsync(id, userId, isTeacher);

        return Ok(submission);
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<ActionResult<List<SubmissionDto>>> GetSubmissionsForCourse(int courseId)
    {
        // Kanske inte behövs när vi har Authorize
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new TokenValidationException();

        var isTeacher = User.IsInRole("Teacher");

        var result = await _serviceManager.SubmissionService.GetSubmissionsForCourseAsync(courseId, userId, isTeacher);

        return Ok(result);
    }

    [HttpGet("activity/{activityId:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<List<SubmissionListItemDto>>> GetSubmissionsForActivity(int activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            throw new TokenValidationException();

        var isTeacher = User.IsInRole("Teacher");

        var result = await _serviceManager.SubmissionService
            .GetSubmissionsForActivityAsync(activityId, userId, isTeacher);

        return Ok(result.ToList());
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<List<SubmissionListItemDto>>> GetAllSubmissions()
    {
        var result = await _serviceManager.SubmissionService.GetAllSubmissionsAsync();
        return Ok(result.ToList());
    }

    [HttpPut("{id:int}/feedback")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GiveFeedback(int id, [FromBody] FeedbackDto dto)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new TokenValidationException();

        if (!ModelState.IsValid)
            throw new BadRequestException("Ogiltiga data skickades för feedback.", "Valideringsfel");

        await _serviceManager.SubmissionService
            .GiveFeedbackAsync(id, dto.Feedback, teacherId);

        return Ok(new { success = true });
    }

    [HttpPost("{activityId:int}")]
    [Authorize(Roles = "Student")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<SubmissionDto>> Submit(
        int activityId,
        [FromForm] SubmissionUploadForm form)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            throw new TokenValidationException();

        if (form.File == null || form.File.Length == 0)
            throw new BadRequestException("Ingen fil vald.", "Valideringsfel");

        // Store in organized path: submissions/{studentId}/{activityId}/{guid}_{originalName}
        var storageName = $"submissions/{studentId}/{activityId}/{Guid.NewGuid()}_{form.File.FileName}";

        using var stream = form.File.OpenReadStream();
        var filePath = await _fileStorage.SaveFileAsync(stream, storageName);

        var result = await _serviceManager.SubmissionService
            .SubmitAsync(activityId, studentId, filePath, form.File.FileName, form.Comment);

        return Ok(result);
    }

    [HttpGet("{id:int}/download")]
    [Authorize]
    public async Task<IActionResult> DownloadFile(int id)
    {
        var fileInfo = await _serviceManager.SubmissionService
            .GetSubmissionFileInfoAsync(id);

        var stream = _fileStorage.OpenReadStream(fileInfo.FilePath);
        if (stream == null)
            throw new InvalidOperationException("Filen hittades inte.");

        return File(stream, "application/octet-stream", fileInfo.FileName);
    }
}