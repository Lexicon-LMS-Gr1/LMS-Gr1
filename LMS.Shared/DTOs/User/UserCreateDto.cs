using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO for creating a new user (Teacher or Student)
/// </summary>
public class UserCreateDto
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ \-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens and apostrophes")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ \-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens and apostrophes")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    [RegularExpression(@"^(?=.*[a-zåäö])(?=.*[A-ZÅÄÖ])(?=.*\d).{6,}$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one number")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(Teacher|Student)$", ErrorMessage = "Role must be either 'Teacher' or 'Student'")]
    public string Role { get; set; } = string.Empty;

    // Required for Students, must be null for Teachers
    public int? CourseId { get; set; }

    // Business rules validated in service layer:
    // 1. Email must be unique
    // 2. If Role is "Student", CourseId must be provided and must exist
    // 3. If Role is "Teacher", CourseId must be null
    // 4. Username will be generated from email (part before @)
}
