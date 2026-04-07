using Microsoft.AspNetCore.Http;

namespace LMS.Presentation.Models;

/// <summary>
/// Form model for document upload. Used by Swagger to generate the correct schema.
/// </summary>
public class DocumentUploadForm
{
    public IFormFile File { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CourseId { get; set; }
    public int? ModuleId { get; set; }
    public int? ActivityId { get; set; }
}
