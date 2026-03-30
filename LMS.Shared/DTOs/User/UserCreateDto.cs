using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.User;

/// <summary>
/// DTO för att skapa en ny användare (Lärare eller Elev)
/// </summary>
public class UserCreateDto
{
    [Required(ErrorMessage = "Förnamn krävs.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Förnamn måste vara mellan 2 och 50 tecken.")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ \-']+$", ErrorMessage = "Förnamn får endast innehålla bokstäver, mellanslag, bindestreck och apostrofer.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Efternamn krävs.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Efternamn måste vara mellan 2 och 50 tecken.")]
    [RegularExpression(@"^[a-zA-ZåäöÅÄÖ \-']+$", ErrorMessage = "Efternamn får endast innehålla bokstäver, mellanslag, bindestreck och apostrofer.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E‑postadress krävs.")]
    [EmailAddress(ErrorMessage = "Ogiltigt e‑postformat.")]
    [StringLength(100, ErrorMessage = "E‑postadressen får inte överstiga 100 tecken.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lösenord krävs.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Lösenord måste vara minst 6 tecken.")]
    [RegularExpression(@"^(?=.*[a-zåäö])(?=.*[A-ZÅÄÖ])(?=.*\d).{6,}$",
        ErrorMessage = "Lösenord måste innehålla minst en versal, en gemen och en siffra.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Roll krävs.")]
    [RegularExpression("^(Teacher|Student)$", ErrorMessage = "Roll måste vara antingen 'Teacher' eller 'Student'.")]
    public string Role { get; set; } = string.Empty;

    // Krävs för elever, måste vara null för lärare
    public int? CourseId { get; set; }
}
