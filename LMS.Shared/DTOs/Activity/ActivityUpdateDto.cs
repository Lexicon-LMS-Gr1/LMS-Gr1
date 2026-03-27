using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Activity;

public class ActivityUpdateDto : ActivityBaseDto
{
    [Required(ErrorMessage = "Aktivitets-Id saknas")]
    public int Id { get; set; }
        
    //[Required(ErrorMessage = "Module ID is required")]
    //[Range(1, int.MaxValue, ErrorMessage = "Module ID must be a positive number")]
    //public int ModuleId { get; set; }

    // Business rules validated in service layer:
    // 1. StartTime must be >= Module.StartDate
    // 2. EndTime must be <= Module.EndDate
    // 3. EndTime must be > StartTime
    // 4. Activity times must NOT overlap with other activities in the same module (excluding itself)
    // 5. DueDate (if provided) should be <= EndTime
    // 6. ActivityTypeId must exist in ActivityType table
}
