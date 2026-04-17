using Domain.Models.Exceptions;
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
    public async Task<ActionResult<ActivityDto>> UpdateActivity(int id, [FromBody] ActivityUpdateDto dto)
    {
        if (id != dto.Id)
            throw new BadRequestException("Id i URL matchar inte aktivitetens id.", "Valideringsfel");

        if (!ModelState.IsValid)
            throw new BadRequestException("Ogiltigt data skickades för aktiviteten.", "Valideringsfel");

        var result = await _serviceManager.ActivityService.UpdateActivityAsync(dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        await _serviceManager.ActivityService.DeleteActivityAsync(id);
        return NoContent();
    }
}