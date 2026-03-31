using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Shared.DTOs.Document;
using Microsoft.AspNetCore.Hosting;
using Service.Contracts;

namespace LMS.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWebHostEnvironment _environment;

    // Allowed file extensions
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
        ".txt", ".csv", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"
    };

    // Max file size: 50 MB
    private const long MaxFileSize = 50 * 1024 * 1024;

    public DocumentService(IUnitOfWork unitOfWork, IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _environment = environment;
    }

    public async Task<DocumentDto> UploadAsync(
        DocumentCreateDto dto,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        string uploadedByUserId)
    {
        // Validate exactly one parent FK is set
        var parentCount = new[] { dto.CourseId.HasValue, dto.ModuleId.HasValue, dto.ActivityId.HasValue }
            .Count(x => x);

        if (parentCount != 1)
            throw new ArgumentException("Exactly one of CourseId, ModuleId, or ActivityId must be provided.");

        // Validate file size
        if (fileSize > MaxFileSize)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)} MB.");

        // Validate file extension
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new ArgumentException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        // Create uploads directory if it doesn't exist
        var uploadsDir = Path.Combine(_environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);

        // Generate unique filename to prevent collisions
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, storedFileName);

        // Save file to disk
        using (var outputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(outputStream);
        }

        // Create entity
        var document = new Document
        {
            Name = dto.Name,
            Description = dto.Description,
            UploadTimestamp = DateTime.UtcNow,
            FilePath = $"uploads/{storedFileName}",
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedByUserId = uploadedByUserId,
            CourseId = dto.CourseId,
            ModuleId = dto.ModuleId,
            ActivityId = dto.ActivityId
        };

        _unitOfWork.DocumentRepository.Create(document);
        await _unitOfWork.CompleteAsync();

        return MapToDto(document);
    }

    public async Task<DocumentDto?> GetByIdAsync(int id)
    {
        var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id);
        return document == null ? null : MapToDto(document);
    }

    public async Task<IEnumerable<DocumentDto>> GetByCourseIdAsync(int courseId)
    {
        var documents = await _unitOfWork.DocumentRepository.GetByCourseIdAsync(courseId);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> GetByModuleIdAsync(int moduleId)
    {
        var documents = await _unitOfWork.DocumentRepository.GetByModuleIdAsync(moduleId);
        return documents.Select(MapToDto);
    }

    public async Task<IEnumerable<DocumentDto>> GetByActivityIdAsync(int activityId)
    {
        var documents = await _unitOfWork.DocumentRepository.GetByActivityIdAsync(activityId);
        return documents.Select(MapToDto);
    }

    public async Task<(Stream FileStream, string ContentType, string FileName)?> DownloadAsync(int id)
    {
        var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id);
        if (document == null)
            return null;

        var fullPath = Path.Combine(_environment.ContentRootPath, document.FilePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found on disk: {document.FilePath}");

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return (stream, document.ContentType, document.FileName);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id, trackChanges: true);
        if (document == null)
            return false;

        // Delete file from disk
        var fullPath = Path.Combine(_environment.ContentRootPath, document.FilePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        // Delete DB record
        _unitOfWork.DocumentRepository.Delete(document);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    private static DocumentDto MapToDto(Document document)
    {
        return new DocumentDto
        {
            Id = document.Id,
            Name = document.Name,
            Description = document.Description ?? string.Empty,
            UploadTimestamp = document.UploadTimestamp,
            UploadedByUserId = document.UploadedByUserId,
            UploadedByUserName = document.UploadedBy != null
                ? $"{document.UploadedBy.FirstName} {document.UploadedBy.LastName}"
                : string.Empty,
            FilePath = document.FilePath,
            FileExtension = Path.GetExtension(document.FileName),
            FileSizeBytes = document.FileSize,
            CourseId = document.CourseId,
            CourseName = document.Course?.Name,
            ModuleId = document.ModuleId,
            ModuleName = document.Module?.Name,
            ActivityId = document.ActivityId,
            ActivityName = document.Activity?.Name
        };
    }
}
