using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO för att uppdatera en befintlig användare.
/// Obs: Lösenordsändring hanteras via ett separat endpoint.
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

    // Endast för elever – används för att flytta elev till en annan kurs
    public int? CourseId { get; set; }
}
