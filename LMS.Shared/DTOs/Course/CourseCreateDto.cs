using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Course;

public class CourseCreateDto
{
    [Required(ErrorMessage = "Kursnamn måste anges.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Kursnamn måste vara mellan 3 och 100 tecken")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivning måste anges.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Kursbeskrivning måste vara mellan 10 och 500 tecken")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Startdatum måste anges.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Slutdatum måste anges.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(6);

    // Custom validation will be done in service layer:
    // - EndDate must be after StartDate
}
