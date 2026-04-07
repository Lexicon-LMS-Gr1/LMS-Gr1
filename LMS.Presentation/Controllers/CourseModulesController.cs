using LMS.Shared.DTOs.Course;
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
        try
        {
            var created = await _moduleService.CreateModuleAsync(courseId, dto);
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
            return StatusCode(500, new { message = "Ett oväntat fel uppstod vid skapande av modul." });
        }
    }
}