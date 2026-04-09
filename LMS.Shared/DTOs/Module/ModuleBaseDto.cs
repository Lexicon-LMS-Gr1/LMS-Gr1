using LMS.Shared.Validation;
using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Module;

public class ModuleBaseDto
{
    [Required(ErrorMessage = "Modulnamn måste anges.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Modulnamn måste vara mellan 3 och 100 tecken.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivning måste anges.")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Modulbeskrivning måste vara mellan 10 och 500 tecken.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Startdatum måste anges.")]
    [DateLessThanOrEqualToOtherDate(nameof(EndDate), ErrorMessage = "Startdatum får inte vara senare än slutdatum.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Slutdatum måste anges.")]
    [DateGreatherThanOrEqualToOtherDate(nameof(StartDate), ErrorMessage = "Slutdatum får inte vara tidigare än startdatum.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    // Business rules validated in service layer:
    // 1. StartDate must be >= Course.StartDate
    // 2. EndDate must be <= Course.EndDate
    // 3. EndDate must be > StartDate
    // 4. Module dates must NOT overlap with other modules in the same course (excluding itself)
}
