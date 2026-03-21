using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Course;

public class CourseUpdateDto
{
    [Required(ErrorMessage = "Course ID is required")]
    public int Id { get; set; }

    [Required(ErrorMessage = "Course name is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Course name must be between 3 and 100 characters")]
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

    // Custom validation will be done in service layer:
    // - EndDate must be after StartDate
    // - Cannot change dates if modules exist (optional business rule)
}
