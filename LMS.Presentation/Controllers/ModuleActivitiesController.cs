using LMS.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/modules/{moduleId:int}/activities")]
[Authorize(Roles = "Teacher")]
public class ModuleActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ModuleActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByModuleId(int moduleId)
    {
        var activities = await _activityService.GetActivitiesByModuleIdAsync(moduleId);
        return Ok(activities);
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(int moduleId, [FromBody] ActivityCreateDto dto)
    {
        try
        {
            var created = await _activityService.CreateActivityAsync(moduleId, dto);
            return Ok(created);
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
            return StatusCode(500, new { message = "Ett oväntat fel uppstod vid skapande av aktivitet." });
        }
    }
}