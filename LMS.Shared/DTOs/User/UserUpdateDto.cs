using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO for updating an existing user
/// Note: Password change should be handled separately via a different endpoint
/// </summary>
public class UserUpdateDto
{
    [Required(ErrorMessage = "Användar-id måste anges.")]
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Förnamn måste anges.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Förnamn måste vara mellan 2 och 50 tecken.")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ \-']+$", ErrorMessage = "Förnamn får endast innehålla bokstäver, mellanslag, bindestreck och apostrofer.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Efternamn måste anges.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Efternamn måste vara mellan 2 och 50 tecken.")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ \-']+$", ErrorMessage = "Efternamn får endast innehålla bokstäver, mellanslag, bindestreck och apostrofer.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-postadress måste anges.")]
    [EmailAddress(ErrorMessage = "E-postadress har felaktigt format.")]
    [StringLength(100, ErrorMessage = "E-postadress kan inte överstiga 100 tecken.")]
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
