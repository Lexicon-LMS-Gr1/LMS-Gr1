using Domain.Models.Entities;
using LMS.Shared.DTOs.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;

namespace LMS.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserManagementService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users
            .Include(u => u.Course)
            .ToListAsync();

        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToDto(user, roles.FirstOrDefault() ?? string.Empty));
        }

        return userDtos;
    }

    public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
    {
        var usersInRole = await _userManager.GetUsersInRoleAsync(role);

        var userDtos = new List<UserDto>();
        foreach (var user in usersInRole)
        {
            userDtos.Add(MapToDto(user, role));
        }

        return userDtos;
    }

    public async Task<IEnumerable<UserDto>> GetUsersByCourseIdAsync(int courseId)
    {
        var users = await _userManager.Users
            .Include(u => u.Course)
            .Where(u => u.CourseId == courseId)
            .ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(MapToDto(user, roles.FirstOrDefault() ?? string.Empty));
        }

        return userDtos;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.Course)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? string.Empty);
    }

    public async Task<UserDto> CreateUserAsync(UserCreateDto dto)
    {
        // Validate role exists
        if (!await _roleManager.RoleExistsAsync(dto.Role))
            throw new ArgumentException($"Role '{dto.Role}' does not exist.");

        // Validate email uniqueness
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            throw new ArgumentException($"A user with email '{dto.Email}' already exists.");

        // Validate business rules
        if (dto.Role == "Teacher" && dto.CourseId.HasValue)
            throw new ArgumentException("A Teacher cannot be assigned to a course.");

        if (dto.Role == "Student" && !dto.CourseId.HasValue)
            throw new ArgumentException("A Student must be assigned to a course.");

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            CourseId = dto.Role == "Student" ? dto.CourseId : null
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            // Rollback: delete the created user
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to assign role: {errors}");
        }

        return MapToDto(user, dto.Role);
    }

    public async Task<UserDto> UpdateUserAsync(UserUpdateDto dto)
    {
        var user = await _userManager.Users
            .Include(u => u.Course)
            .FirstOrDefaultAsync(u => u.Id == dto.Id);

        if (user == null)
            throw new KeyNotFoundException($"User with ID '{dto.Id}' not found.");

        // Validate email uniqueness if changed
        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new ArgumentException($"A user with email '{dto.Email}' already exists.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault() ?? string.Empty;

        // Teachers cannot have CourseId
        if (currentRole == "Teacher" && dto.CourseId.HasValue)
            throw new ArgumentException("A Teacher cannot be assigned to a course.");

        if (currentRole == "Student" && !dto.CourseId.HasValue)
            throw new ArgumentException("A Student must be assigned to a course.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.CourseId = currentRole == "Student" ? dto.CourseId : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to update user: {errors}");
        }

        return MapToDto(user, currentRole);
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        // Prevent deleting the last teacher
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Teacher"))
        {
            var teacherCount = (await _userManager.GetUsersInRoleAsync("Teacher")).Count;
            if (teacherCount <= 1)
                throw new InvalidOperationException(
                    "Cannot delete the last teacher in the system. At least one teacher must exist.");
        }

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task DeleteStudentsByCourseAsync(int courseId)
    {
        var students = await _userManager.Users
            .Where(u => u.CourseId == courseId)
            .ToListAsync();

        foreach (var student in students)
        {
            await _userManager.DeleteAsync(student);
        }
    }


    private static UserDto MapToDto(ApplicationUser user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Role = role,
            CourseId = user.CourseId,
            CourseName = user.Course?.Name
        };
    }
}
