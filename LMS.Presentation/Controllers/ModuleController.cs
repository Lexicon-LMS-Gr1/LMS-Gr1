using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Module;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/module")]
[Authorize(Roles = "Teacher")]
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
            throw new BadRequestException("Modul-id stämmer inte.", "Valideringsfel");

        if (!ModelState.IsValid)
            throw new BadRequestException("Ogiltigt data skickades för modulen.", "Valideringsfel");

        var updated = await _serviceManager.ModuleService.UpdateModuleAsync(dto);

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteModule(int id)
    {
        await _serviceManager.ModuleService.DeleteModuleAsync(id);

        return NoContent();
    }
}