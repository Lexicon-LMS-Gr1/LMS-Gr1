namespace LMS.Shared.DTOs.Document;

/// <summary>
/// DTO for displaying document information
/// </summary>
public class DocumentDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime UploadTimestamp { get; set; }
    
    public string UploadedByUserId { get; set; } = string.Empty;
    
    public string UploadedByUserName { get; set; } = string.Empty;
    
    public string FilePath { get; set; } = string.Empty;
    
    public string FileExtension { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    // Parent entity information (exactly one will be set)
    public int? CourseId { get; set; }
    public string? CourseName { get; set; }
    
    public int? ModuleId { get; set; }
    public string? ModuleName { get; set; }
    
    public int? ActivityId { get; set; }
    public string? ActivityName { get; set; }
}
