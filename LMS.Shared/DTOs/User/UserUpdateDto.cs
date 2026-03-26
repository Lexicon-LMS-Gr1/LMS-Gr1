using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO for updating an existing user
/// Note: Password change should be handled separately via a different endpoint
/// </summary>
public class UserUpdateDto
{
    [Required(ErrorMessage = "User ID is required")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens and apostrophes")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens and apostrophes")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    // For students only - can be used to move student to another course
    public int? CourseId { get; set; }

    // Business rules validated in service layer:
    // 1. User with given Id must exist
    // 2. Email must be unique (if changed)
    // 3. Cannot change a Teacher's CourseId (should remain null)
    // 4. If user is Student and CourseId is provided, course must exist
    // 5. Cannot change user's role via this DTO
}
