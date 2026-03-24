using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public UserController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("me/course")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCourse()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var course = await _courseService.GetCourseForUserAsync(userId);

            if (course == null)
                return NotFound("Ingen kurs hittades för denna elev.");

            return Ok(course);
        }

        [HttpGet("me/course/participants")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCourseParticipants()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var participants = await _courseService.GetParticipantsForUserCourseAsync(userId);

            return Ok(participants);
        }
    }
}