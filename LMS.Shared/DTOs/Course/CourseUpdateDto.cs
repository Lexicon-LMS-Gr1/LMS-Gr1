using System.ComponentModel.DataAnnotations;
using LMS.Shared.Validation;

namespace LMS.Shared.DTOs.Course;

public class CourseUpdateDto
{
    [Required(ErrorMessage = "Kurs-id måste anges.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Kursnamn måste anges.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kursnamn måste vara mellan 3 och 100 tecken.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivning måste anges.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Kursbeskrivning måste vara mellan 10 och 500 tecken.")]
    public string Description { get; set; } = string.Empty;
    public string? TeacherId { get; set; }

    [Required(ErrorMessage = "Startdatum måste anges.")]
    [DateLessThanOrEqualToOtherDate(nameof(EndDate), ErrorMessage = "Startdatum får inte vara senare än slutdatum.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Slutdatum måste anges.")]
    [DateGreatherThanOrEqualToOtherDate(nameof(StartDate), ErrorMessage = "Slutdatum får inte vara tidigare än startdatum.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    // Custom validation will be done in service layer:
    // - EndDate must be after StartDate
    // - Cannot change dates if modules exist (optional business rule)
}
