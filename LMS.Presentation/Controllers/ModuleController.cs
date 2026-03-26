using LMS.Shared.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/module")]
public class ModuleController : ControllerBase
{
	private readonly IServiceManager _serviceManager;

	public ModuleController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
	}

	[Authorize(Roles = "Teacher")]
	[HttpGet("{moduleId}/activities")]
	public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities(int moduleId)
	{
		var activities = await _serviceManager.ModuleService.GetActivitiesAsync(moduleId);
		return Ok(activities);
	}
}