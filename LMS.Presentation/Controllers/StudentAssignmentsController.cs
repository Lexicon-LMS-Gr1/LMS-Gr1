using Domain.Models.Entities;
using Domain.Models.Exceptions;
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

            if (user == null)
                throw new ForbiddenException("Användaren kunde inte identifieras.");

            if (user.CourseId == null)
                throw new BadRequestException("Eleven är inte kopplad till någon kurs.", "Valideringsfel");

            var result = await _serviceManager.StudentAssignmentService
                .GetStudentAssignmentsAsync(user.Id, user.CourseId.Value);

            return Ok(result);
        }

        // GET api/student/assignments/upcoming
        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingAssignments()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                throw new ForbiddenException("Användaren kunde inte identifieras.");

            if (user.CourseId == null)
                throw new BadRequestException("Eleven är inte kopplad till någon kurs.", "Valideringsfel");

            var result = await _serviceManager.StudentAssignmentService
                .GetUpcomingAssignmentsAsync(user.Id);

            return Ok(result);
        }

    }

}
