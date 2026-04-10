using Domain.Contracts.Services;
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
		if (string.IsNullOrEmpty(userId)) {
			return Unauthorized();
		}

		var isTeacher = User.IsInRole("Teacher");

		var submission = await _serviceManager.SubmissionService.GetSubmissionByIdAsync(id,userId,isTeacher);

		if (submission is null)
			return NotFound();

		return Ok(submission);
	}

	[HttpGet("course/{courseId:int}")]
	public async Task<ActionResult<List<SubmissionDto>>> GetSubmissionsForCourse(int courseId)
	{
		// Kanske inte behövs när vi har Authorize
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
		if (string.IsNullOrEmpty(userId)) {
			return Unauthorized();
		}
		var isTeacher = User.IsInRole("Teacher");

		var result = await _serviceManager.SubmissionService.GetSubmissionsForCourseAsync(courseId, userId, isTeacher);

		return Ok(result);
	}

    [HttpGet("activity/{activityId:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<List<SubmissionListItemDto>>> GetSubmissionsForActivity(int activityId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var isTeacher = User.IsInRole("Teacher");
        if (!isTeacher)
            return Forbid();

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
        if (string.IsNullOrEmpty(teacherId))
            return Unauthorized();

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
        if (string.IsNullOrEmpty(studentId))
            return Unauthorized();

        if (form.File == null || form.File.Length == 0)
            return BadRequest("Ingen fil vald.");

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

        if (fileInfo == null)
            return NotFound();

        var stream = _fileStorage.OpenReadStream(fileInfo.Value.FilePath);
        if (stream == null)
            return NotFound("Filen hittades inte.");

        return File(stream, "application/octet-stream", fileInfo.Value.FileName);
    }

}
