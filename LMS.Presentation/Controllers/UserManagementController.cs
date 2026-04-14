using Domain.Models.Exceptions;
using LMS.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Teacher")]
public class UserManagementController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public UserManagementController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _serviceManager.UserManagementService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        var user = await _serviceManager.UserManagementService.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpGet("role/{role}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string role)
    {
        var users = await _serviceManager.UserManagementService.GetUsersByRoleAsync(role);
        return Ok(users);
    }

    [HttpGet("course/{courseId:int}")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByCourse(int courseId)
    {
        var users = await _serviceManager.UserManagementService.GetUsersByCourseIdAsync(courseId);
        return Ok(users);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
            throw new BadRequestException("Ogiltigt data skickades för användaren.", "Valideringsfel");

        var user = await _serviceManager.UserManagementService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UserUpdateDto dto)
    {
        if (!ModelState.IsValid)
            throw new BadRequestException("Ogiltigt data skickades för användaren.", "Valideringsfel");

        if (id != dto.Id)
            throw new BadRequestException("Id i URL:en matchar inte id i request body.", "Valideringsfel");

        var user = await _serviceManager.UserManagementService.UpdateUserAsync(dto);
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Prevent a teacher from deleting their own account
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != null && currentUserId == id)
            throw new BadRequestException("Du kan inte ta bort ditt eget konto.", "Valideringsfel");

        await _serviceManager.UserManagementService.DeleteUserAsync(id);

        return NoContent();
    }
}
