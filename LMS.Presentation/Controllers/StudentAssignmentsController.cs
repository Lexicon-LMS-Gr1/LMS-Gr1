using Domain.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Presentation.Controllers
{
    [Authorize(Roles = "Student")]
    [ApiController]
    [Route("api/student/assignments")]
    public class StudentAssignmentsController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentAssignmentsController(
            IServiceManager serviceManager,
            UserManager<ApplicationUser> userManager)
        {
            _serviceManager = serviceManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAssignments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user.CourseId == null)
                return BadRequest("Student is not assigned to any course.");

            var result = await _serviceManager.StudentAssignmentService
                .GetStudentAssignmentsAsync(user.Id, user.CourseId.Value);

            return Ok(result);
        }


    }

}
