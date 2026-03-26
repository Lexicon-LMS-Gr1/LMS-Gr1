using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Module;

public class ModuleCreateDto
{
    [Required(ErrorMessage = "Modulnamn saknas")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Modulnamn måste vara mellan 3 och 100 tecken")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Beskrivning saknas")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Beskrivning måste vara mellan 10 och 500 tecken")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Startdatum saknas")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Slutdatum saknas")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }
}

