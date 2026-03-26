using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;
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

	[HttpGet("{moduleId}/activities")]
	public async Task<ActionResult<IEnumerable<ActivityDto>>> GetActivities(int moduleId)
	{
		var activities = await _serviceManager.ModuleService.GetActivitiesAsync(moduleId);
		return Ok(activities);
	}

    [HttpPut("{moduleId}")]
    public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] ModuleUpdateDto dto)
    {
        if (moduleId != dto.Id)
            return BadRequest("Id mismatch");

        var updated = await _serviceManager.ModuleService.UpdateModuleAsync(dto);

        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

}