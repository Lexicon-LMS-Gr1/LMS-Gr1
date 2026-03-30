using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO för att skapa en ny användare (Lärare eller Elev)
/// </summary>
public class UserCreateDto
{
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

    [Required(ErrorMessage = "Lösenord måste anges.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenord måste vara minst 6 tecken.")]
    [RegularExpression(@"^(?=.*[a-zåäö])(?=.*[A-ZÅÄÖ])(?=.*\d).{6,}$", 
        ErrorMessage = "Lösenord måste innehålla åtminstone en versal, en gemen och en siffra.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Roll måste anges.")]
    [RegularExpression("^(Teacher|Student)$", ErrorMessage = "Roll måste vara antingen \"Teacher\" eller \"Student\".")]
    public string Role { get; set; } = string.Empty;

    // Krävs för elever, måste vara null för lärare
    public int? CourseId { get; set; }
}
