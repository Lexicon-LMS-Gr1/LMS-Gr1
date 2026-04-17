using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Module;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/modules")]
[Authorize(Roles = "Teacher")]
public class CourseModulesController : ControllerBase
{
    private readonly IModuleService _moduleService;

    public CourseModulesController(IModuleService moduleService)
    {
        _moduleService = moduleService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ModuleDto>>> GetByCourseId(int courseId)
    {
        var modules = await _moduleService.GetModulesByCourseIdAsync(courseId);
        return Ok(modules);
    }

    [HttpPost]
    public async Task<ActionResult<ModuleDto>> Create(int courseId, [FromBody] ModuleCreateDto dto)
    {
        if (!ModelState.IsValid)
            throw new BadRequestException("Ogiltiga data skickades för modulen.", "Valideringsfel");

        var created = await _moduleService.CreateModuleAsync(courseId, dto);
        return Ok(created);
    }
}