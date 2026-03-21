using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Activity;

public class ActivityCreateDto
{
    [Required(ErrorMessage = "Activity name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Activity name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start time is required")]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "End time is required")]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Required(ErrorMessage = "Activity type is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Activity type ID must be a positive number")]
    public int ActivityTypeId { get; set; }

    [Required(ErrorMessage = "Module ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Module ID must be a positive number")]
    public int ModuleId { get; set; }

    // Business rules validated in service layer:
    // 1. StartTime must be >= Module.StartDate
    // 2. EndTime must be <= Module.EndDate
    // 3. EndTime must be > StartTime
    // 4. Activity times must NOT overlap with other activities in the same module
    // 5. DueDate (if provided) should be <= EndTime
    // 6. ActivityTypeId must exist in ActivityType table
}
