using LMS.Shared.DTOs.User;

namespace Service.Contracts;

public interface IUserManagementService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role);
    Task<IEnumerable<UserDto>> GetUsersByCourseIdAsync(int courseId);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<UserDto> CreateUserAsync(UserCreateDto userDto);
    Task<UserDto> UpdateUserAsync(UserUpdateDto userDto);
    Task<bool> DeleteUserAsync(string id);
    Task DeleteStudentsByCourseAsync(int courseId);
    Task<IEnumerable<UserDto>> GetTeachersAsync();
}
