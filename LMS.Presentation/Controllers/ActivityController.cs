using LMS.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/activity")]
[Authorize(Roles = "Teacher")]
public class ActivityController : ControllerBase
{
	private readonly IActivityService _activityService;

	public ActivityController(IActivityService activityService)
	{
		_activityService = activityService;
	}

	[HttpPut("{activityId:int}")]
	public async Task<IActionResult> UpdateActivity(int activityId, [FromBody] ActivityUpdateDto dto)
	{
		if (activityId != dto.Id)
            return BadRequest("Aktivitets-id stämmer inte.");

		try
		{
			var updated = await _activityService.UpdateActivityAsync(dto);
			return Ok(updated);
		}
		catch (ArgumentException ex)
		{
			return BadRequest(new { message = ex.Message });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { message = ex.Message });
		}
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ett oväntat fel uppstod vid uppdatering av aktivitet." });
        }
    }
}
