using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public UsersController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("me/course")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCourse()
        {
            /*
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var course = await _courseService.GetCourseForUserAsync(userId);

            if (course == null)
                return NotFound("Ingen kurs hittades för denna elev.");

            return Ok(course);
            */
            return Ok();
        }
    }
}