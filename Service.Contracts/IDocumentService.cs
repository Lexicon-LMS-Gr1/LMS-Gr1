using LMS.Shared.DTOs.Document;

namespace Service.Contracts;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetDocumentsByCourseIdAsync(int courseId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByModuleIdAsync(int moduleId);
    Task<IEnumerable<DocumentDto>> GetDocumentsByActivityIdAsync(int activityId);
    Task<DocumentDto?> GetDocumentByIdAsync(int id);
    Task<DocumentDto> CreateDocumentAsync(DocumentCreateDto documentDto, string filePath, string uploadedByUserId);
    Task<bool> DeleteDocumentAsync(int id);
    Task<string?> GetDocumentFilePathAsync(int id);
}
