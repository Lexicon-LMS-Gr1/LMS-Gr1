using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Module;

public class ModuleUpdateDto : ModuleBaseDto
{
    [Required(ErrorMessage = "Modul-id måste anges.")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Kurs-id måste anges.")]
    [Range(1, int.MaxValue, ErrorMessage = "Kurs-id ska vara ett positivt tal.")]
    public int CourseId { get; set; }
}
