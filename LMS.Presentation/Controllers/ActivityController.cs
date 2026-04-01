using LMS.Shared.DTOs.Activity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/activity")]
[Authorize(Roles = "Teacher")]
public class ActivityController : ControllerBase
{
	private readonly IServiceManager _serviceManager;

	public ActivityController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<ActivityDto>> UpdateActivity(int id, [FromBody] UpdateActivityDto dto)
	{
		if (id != dto.Id)
			return BadRequest();
		if (!ModelState.IsValid)
			return BadRequest(ModelState);


		try {
			var result = await _serviceManager.ActivityService.UpdateActivityAsync2(dto);
			return Ok(result);
		} catch (ArgumentException ex) {
			return BadRequest(ex.Message);
		} catch (KeyNotFoundException) {
			return NotFound();
		}
	}

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var success = await _serviceManager.ActivityService.DeleteActivityAsync(id);

        if (!success)
            return NotFound();

        return NoContent();
    }

}