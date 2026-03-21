namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO for displaying user information
/// </summary>
public class UserDto
{
    public string Id { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string FullName => $"{FirstName} {LastName}";
    
    public string Email { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    
    public string Role { get; set; } = string.Empty;
    
    // For students only
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
}
