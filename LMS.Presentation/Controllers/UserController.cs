using LMS.Shared.DTOs.StudentDashboard;
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
        private readonly IServiceManager _serviceManager;

        public UserController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpGet("me/dashboard")]
        public async Task<ActionResult<StudentDashboardDto>> GetDashboard()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized();

            var dashboard = await _serviceManager.DashboardService.GetDashboardAsync(userId);
            return Ok(dashboard);
        }

        [HttpGet("me/course")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyCourse()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized();

            var course = await _serviceManager.CourseService.GetCourseForUserAsync(userId);

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

            var participants = await _serviceManager.CourseService.GetParticipantsForUserCourseAsync(userId);

            return Ok(participants);
        }
    }
}