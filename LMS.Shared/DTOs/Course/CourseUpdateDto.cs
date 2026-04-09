using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Course;

public class CourseUpdateDto : CourseBaseDto
{
    [Required(ErrorMessage = "Kurs-id måste anges.")]
    public int Id { get; set; }

    // Custom validation will be done in service layer:
    // - Cannot change dates if modules exist (optional business rule)
}
