using LMS.Shared.DTOs.Document;

namespace Service.Contracts;

public interface IDocumentService
{
    /// <summary>Upload a document and save its file to disk.</summary>
    Task<DocumentDto> UploadAsync(DocumentCreateDto dto, Stream fileStream, string fileName, string contentType, long fileSize, string uploadedByUserId);

    /// <summary>Get document metadata by ID.</summary>
    Task<DocumentDto> GetByIdAsync(int id);

    /// <summary>List documents by parent entity.</summary>
    Task<IEnumerable<DocumentDto>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<DocumentDto>> GetByModuleIdAsync(int moduleId);
    Task<IEnumerable<DocumentDto>> GetByActivityIdAsync(int activityId);

    /// <summary>Get file stream for download. Returns null if document not found.</summary>
    Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(int id);

    /// <summary>Delete a document (removes file from disk + DB record).</summary>
    Task DeleteAsync(int id);
}
