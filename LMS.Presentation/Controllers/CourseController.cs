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
	public async Task<IActionResult> GetCoursesAsList()
	{
		var courses = await _serviceManager.CourseService.GetAllCoursesAsListAsync();
		return Ok(courses);
	}
}
