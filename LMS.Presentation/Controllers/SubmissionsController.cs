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

	public SubmissionsController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
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

}
