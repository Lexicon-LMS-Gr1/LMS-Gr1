using Domain.Models.Entities;
using Domain.Models.Exceptions;
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

        if (user == null)
            throw new NotFoundException($"Användare med id \"{id}\" hittades inte.");

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? string.Empty);
    }

    public async Task<UserDto> CreateUserAsync(UserCreateDto dto)
    {
        if (dto is null)
            throw new BadRequestException("Användardata saknas.", "Valideringsfel");

        // Validate role exists
        if (!await _roleManager.RoleExistsAsync(dto.Role))
            throw new ArgumentException($"Rollen \"{dto.Role}\" finns inte.");

        // Validate email uniqueness
        if (!await _roleManager.RoleExistsAsync(dto.Role))
            throw new BadRequestException($"Rollen \"{dto.Role}\" finns inte.", "Valideringsfel");

        // Validate email uniqueness
        var existingUserWithSameEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUserWithSameEmail != null)
            throw new BadRequestException($"En användare med e-postadressen \"{dto.Email}\" finns redan.", "Valideringsfel");

        // Validate business rules
        if (dto.Role == "Teacher" && dto.CourseId.HasValue)
            throw new BadRequestException("En lärare kan inte kopplas till en kurs på samma sätt som en elev.", "Valideringsfel");

        if (dto.Role == "Student" && !dto.CourseId.HasValue)
            throw new BadRequestException("En elev måste kopplas till en kurs.", "Valideringsfel");

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
            throw new InvalidOperationException($"Kunde inte skapa användare: {errors}");
        }

        var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
        if (!roleResult.Succeeded)
        {
            // Rollback: delete the created user
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Misslyckades med att tilldela rollen: {errors}");
        }

        return MapToDto(user, dto.Role);
    }

    public async Task<UserDto> UpdateUserAsync(UserUpdateDto dto)
    {
        if (dto is null)
            throw new BadRequestException("Användardata saknas.", "Valideringsfel");

        var user = await _userManager.Users
            .Include(u => u.Course)
            .FirstOrDefaultAsync(u => u.Id == dto.Id);

        if (user == null)
            throw new KeyNotFoundException($"Användare med id \"{dto.Id}\" hittades inte.");

        // Validate email uniqueness if changed
        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingUserWithSameEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUserWithSameEmail != null)
                throw new BadRequestException($"En användare med e-postadressen \"{dto.Email}\" finns redan.", "Valideringsfel");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var currentRole = roles.FirstOrDefault() ?? string.Empty;

        // Teachers cannot have CourseId
        if (currentRole == "Teacher" && dto.CourseId.HasValue)
            throw new BadRequestException("En lärare kan inte kopplas till en kurs på samma sätt som en elev.", "Valideringsfel");

        if (currentRole == "Student" && !dto.CourseId.HasValue)
            throw new BadRequestException("En elev måste kopplas till en kurs.", "Valideringsfel");


        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.Email = dto.Email.Trim();
        user.UserName = dto.Email.Trim();
        user.CourseId = currentRole == "Student" ? dto.CourseId : null;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Kunde inte uppdatera användare: {errors}");
        }

        return MapToDto(user, currentRole);
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            throw new NotFoundException($"Användare med id \"{id}\" hittades inte.");

        // Prevent deleting the last teacher
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Teacher"))
        {
            var teacherCount = (await _userManager.GetUsersInRoleAsync("Teacher")).Count;
            if (teacherCount <= 1)
                throw new InvalidOperationException(
                    "Kan inte ta bort den sista läraren i systemet. Åtminstone en lärare måste finnas.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Kunde inte ta bort användare: {errors}");
        }
    }

    public async Task DeleteStudentsByCourseAsync(int courseId)
    {
        var students = await _userManager.Users
            .Where(u => u.CourseId == courseId)
            .ToListAsync();

        foreach (var student in students)
        {
            var result = await _userManager.DeleteAsync(student);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Kunde inte ta bort elev: {errors}");
            }
        }
    }

    public async Task<IEnumerable<UserDto>> GetTeachersAsync()
    {
        var teachers = await _userManager.GetUsersInRoleAsync("Teacher");

        var result = new List<UserDto>();

        foreach (var teacher in teachers)
        {
            result.Add(MapToDto(teacher, "Teacher"));
        }

        return result;
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
