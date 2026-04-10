using Microsoft.AspNetCore.Http;

namespace LMS.Presentation.Models;

public class SubmissionUploadForm
{
    public IFormFile File { get; set; } = default!;
    public string? Comment { get; set; }
}
