using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Module;

public class ModuleCreateDto
{
    [Required(ErrorMessage = "Module name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Module name must be between 3 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "End date is required")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Course ID is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Course ID must be a positive number")]
    public int CourseId { get; set; }

    // Business rules validated in service layer:
    // 1. StartDate must be >= Course.StartDate
    // 2. EndDate must be <= Course.EndDate
    // 3. EndDate must be > StartDate
    // 4. Module dates must NOT overlap with other modules in the same course
}
