using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Activity;

public class ActivityCreateDto : ActivityBaseDto
{
    [Required(ErrorMessage = "Aktivitetstyp måste anges.")]
    [Range(1, int.MaxValue, ErrorMessage = "Aktivitetstyp måste väljas.")] // (Inte bra meddelande: "Aktivitets-id måste vara ett positivt tal.")
    public int ActivityTypeId { get; set; }

    //[Required(ErrorMessage = "Modul-id måste anges.")]
    //[Range(1, int.MaxValue, ErrorMessage = "Modul-id ska vara ett positivt tal.")]
    //public int ModuleId { get; set; }

    // Business rules validated in service layer:
    // 1. StartTime must be >= Module.StartDate
    // 2. EndTime must be <= Module.EndDate
    // 3. EndTime must be > StartTime
    // 4. Activity times must NOT overlap with other activities in the same module
    // 5. DueDate (if provided) should be <= EndTime
    // 6. ActivityTypeId must exist in ActivityType table
}
