using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Document;

/// <summary>
/// DTO for creating a new document
/// Note: File upload is handled separately via IFormFile in the controller
/// </summary>
public class DocumentCreateDto
{
    [Required(ErrorMessage = "Dokumentnamn måste anges.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Dokumentnamn måste vara mellan 3 och 200 tecken.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Beskrivning får högst vara 500 tecken.")]
    public string? Description { get; set; }

    // Parent entity foreign keys - exactly ONE must be provided
    // Validation done at service level
    public int? CourseId { get; set; }
    public int? ModuleId { get; set; }
    public int? ActivityId { get; set; }

    // Business rules validated in service layer:
    // 1. Exactly one of CourseId, ModuleId, or ActivityId must be provided
    // 2. The parent entity (Course/Module/Activity) must exist
    // 3. File size must not exceed configured limit (e.g., 50MB)
    // 4. File type must be allowed (e.g., .pdf, .docx, .pptx, .xlsx, .txt, .jpg, .png)
}
