using Domain.Contracts.Repositories;
using Domain.Contracts.Services;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Document;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;

namespace LMS.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly UserManager<ApplicationUser> _userManager;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx", ".xls", ".xlsx",
        ".txt", ".csv", ".jpg", ".jpeg", ".png", ".gif", ".zip", ".rar"
    };

    private const long MaxFileSize = 50 * 1024 * 1024;

    public DocumentService(IUnitOfWork unitOfWork, IFileStorageService fileStorage, UserManager<ApplicationUser> userManager)

    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _userManager = userManager;
    }

    public async Task<DocumentDto> UploadAsync(
        DocumentCreateDto dto,
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        string uploadedByUserId)
    {
        if (dto is null)
            throw new BadRequestException("Dokumentdata saknas.", "Valideringsfel");

        if (fileStream is null)
            throw new BadRequestException("Filström saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new BadRequestException("Filnamn saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(uploadedByUserId))
            throw new BadRequestException("Uppladdande användare saknas.", "Valideringsfel");

        var parentCount = new[] { dto.CourseId.HasValue, dto.ModuleId.HasValue, dto.ActivityId.HasValue }
            .Count(x => x);

        if (parentCount != 1)
            throw new BadRequestException("Exakt en av CourseId, ModuleId eller ActivityId måste anges.", "Valideringsfel");

        if (fileSize > MaxFileSize)
            throw new BadRequestException(
                $"Filstorleken överskrider maximal tillåten storlek på {MaxFileSize / (1024 * 1024)} MB.",
                "Valideringsfel");

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            throw new BadRequestException(
                $"Filtypen \"{extension}\" är inte tillåten. Tillåtna filtyper: {string.Join(", ", AllowedExtensions)}",
                "Valideringsfel");

        var relativePath = await _fileStorage.SaveFileAsync(fileStream, fileName);

        var document = new Document
        {
            Name = dto.Name,
            Description = dto.Description,
            UploadTimestamp = DateTime.UtcNow,
            FilePath = relativePath,
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            UploadedByUserId = uploadedByUserId,
            CourseId = dto.CourseId,
            ModuleId = dto.ModuleId,
            ActivityId = dto.ActivityId
        };

        _unitOfWork.DocumentRepository.Create(document);

        // Notification 
        await CreateDocumentNotificationsAsync(dto, document, uploadedByUserId);

        // end

        await _unitOfWork.CompleteAsync();

        return MapToDto(document);
    }

    private async Task CreateDocumentNotificationsAsync(
        DocumentCreateDto dto,
        Document document,
        string uploadedByUserId)
    {
        var uploader = await _userManager.FindByIdAsync(uploadedByUserId);

        // endast lärare ska trigga notiser
        if (uploader is null || !await _userManager.IsInRoleAsync(uploader, "Teacher"))
            return;

        int courseId;
        string? courseName = null;
        string? moduleName = null;
        string? activityName = null;

        // exakt en av CourseId, ModuleId eller ActivityId är satt (validerat med parentCount)
        if (dto.CourseId is int cId)
        {
            courseId = cId;
        }
        else if (dto.ModuleId is int mId)
        {
            var module = await _unitOfWork.ModuleRepository.GetModuleWithCourseAsync(mId)
                ?? throw new NotFoundException("Modulen hittades inte.");

            courseId = module.CourseId;
            moduleName = module.Name;
            courseName = module.Course.Name;
        }
        else
        {
            var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(dto.ActivityId!.Value)
                ?? throw new NotFoundException("Aktiviteten hittades inte.");

            courseId = activity.Module.CourseId;
            activityName = activity.Name;
            courseName = activity.Module.Course.Name;
        }

        var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseId)
            ?? throw new NotFoundException("Kursen hittades inte.");

        // skicka bara notiser om kursen har startat
        if (course.StartDate > DateTime.UtcNow)
            return;

        courseName ??= course.Name;

        // hämta alla användare i kursen
        var students = await _userManager.Users
            .Where(u => u.CourseId == courseId)
            .ToListAsync();

        foreach (var user in students)
        {
            if (await _userManager.IsInRoleAsync(user, "Student"))
            {
                _unitOfWork.NotificationRepository.Create(new Notification
                {
                    UserId = user.Id,
                    Type = NotificationType.DocumentUploaded,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false,

                    ActorUserId = uploader.Id,
                    ActorName = $"{uploader.FirstName} {uploader.LastName}",

                    CourseId = courseId,
                    CourseName = courseName,

                    ModuleId = dto.ModuleId,
                    ModuleName = moduleName,

                    ActivityId = dto.ActivityId,
                    ActivityName = activityName,

                    DocumentId = document.Id,
                    DocumentName = document.Name,

                    Message = document.Description
                });
            }
        }
    }

    public async Task<DocumentDto> GetByIdAsync(int id)
    {
        var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id);

        if (document == null)
            throw new NotFoundException($"Dokument med id {id} hittades inte.");

        return MapToDto(document);
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

    public async Task<(Stream FileStream, string ContentType, string FileName)> DownloadAsync(int id)
    {
        var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id);
        if (document == null)
            throw new NotFoundException($"Dokument med id {id} hittades inte.");

        if (!_fileStorage.FileExists(document.FilePath))
            throw new InvalidOperationException($"Filen hittades inte på disk: {document.FilePath}");

        var stream = _fileStorage.OpenReadStream(document.FilePath);
        return (stream, document.ContentType, document.FileName);
    }

    public async Task DeleteAsync(int id)
    {
        var document = await _unitOfWork.DocumentRepository.GetByIdAsync(id, trackChanges: true);
        if (document == null)
            throw new NotFoundException($"Dokument med id {id} hittades inte.");

        _fileStorage.DeleteFile(document.FilePath);

        _unitOfWork.DocumentRepository.Delete(document);
        await _unitOfWork.CompleteAsync();
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