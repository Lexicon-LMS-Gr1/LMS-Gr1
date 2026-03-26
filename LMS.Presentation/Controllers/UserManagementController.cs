using System.Security.Claims;
using LMS.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

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

        if (user == null)
            return NotFound($"User with ID '{id}' not found.");

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
            return BadRequest(ModelState);

        try
        {
            var user = await _serviceManager.UserManagementService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(string id, [FromBody] UserUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.Id)
            return BadRequest("ID in URL does not match ID in request body.");

        try
        {
            var user = await _serviceManager.UserManagementService.UpdateUserAsync(dto);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        // Prevent a teacher from deleting their own account
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId != null && currentUserId == id)
            return BadRequest("Du kan inte ta bort ditt eget konto.");

        try
        {
            var result = await _serviceManager.UserManagementService.DeleteUserAsync(id);

            if (!result)
                return NotFound($"User with ID '{id}' not found.");

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
