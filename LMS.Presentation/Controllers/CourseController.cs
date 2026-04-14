using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

namespace LMS.Presentation.Controllers;

/*
 * https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Presentation/Controllers/EmployeesController.cs
 */

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Teacher")]
public class CourseController : ControllerBase
{
	private readonly IServiceManager _serviceManager;

	public CourseController(IServiceManager serviceManager)
	{
		this._serviceManager = serviceManager;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<CourseDto>>> GetAllCourses()
	{
		var courses = await _serviceManager.CourseService.GetAllCoursesAsync();
		return Ok(courses);
	}

	[HttpGet("list")]
	public async Task<ActionResult<IEnumerable<CourseListDto>>> GetCoursesList()
	{
		var courses = await _serviceManager.CourseService.GetAllCoursesListAsync();
		return Ok(courses);
	}

	[HttpGet("{courseId}")]
	public async Task<ActionResult<CourseDto>> GetCourseById(int courseId)
	{
		var course = await _serviceManager.CourseService.GetCourseByIdAsync(courseId);
		if (course == null) 
			return NotFound();

		return Ok(course);
	}

    [HttpPost]
    public async Task<ActionResult<CourseDto>> CreateCourse([FromBody] CourseCreateDto courseCreateDto)
    {
        try
        {
            var createdCourse = await _serviceManager.CourseService.CreateCourseAsync(courseCreateDto);
            return Ok(createdCourse);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Ett oväntat fel uppstod vid skapande av kurs." });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CourseDto>> UpdateCourse(int id, [FromBody] CourseUpdateDto dto)
    {
        if (id != dto.Id)
            throw new BadRequestException("Kurs-id stämmer inte.", "Valideringsfel");

        var updated = await _serviceManager.CourseService.UpdateCourseAsync(dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var result = await _serviceManager.CourseService.DeleteCourseAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpGet("teachers")]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await _serviceManager.UserManagementService.GetTeachersAsync();

        return Ok(teachers.Select(t => new {
            t.Id,
            FullName = $"{t.FirstName} {t.LastName}",
            t.Email
        }));
    }



}
