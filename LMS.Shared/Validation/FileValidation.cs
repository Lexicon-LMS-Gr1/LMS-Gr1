namespace LMS.Shared.Validation;

/// <summary>
/// Shared file validation constants used by both frontend and backend.
/// </summary>
public static class FileValidation
{
    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
        ".txt", ".csv", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"
    };

    public const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
}
