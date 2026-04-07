using LMS.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/activitytypes")]
[Authorize(Roles = "Teacher")]
public class ActivityTypesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityTypesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityTypeDto>>> GetAll()
    {
        var activityTypes = await _activityService.GetAllActivityTypesAsync();
        return Ok(activityTypes);
    }
}