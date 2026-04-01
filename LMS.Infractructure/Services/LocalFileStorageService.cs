using Domain.Contracts.Services;
using Microsoft.Extensions.Configuration;

namespace LMS.Infractructure.Services;

/// <summary>
/// Local file-system storage implementation.
/// Reads the base path from configuration ("FileStorage:BasePath") and defaults to "uploads".
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:BasePath"];

        // If the configured path is relative, resolve it against the current directory
        // (which is the API project's content root when hosted by Kestrel).
        _basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);

        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(_basePath, storedFileName);

        using var outputStream = new FileStream(fullPath, FileMode.Create);
        await stream.CopyToAsync(outputStream, ct);

        // Return relative path for DB storage
        return $"uploads/{storedFileName}";
    }

    public Stream OpenReadStream(string relativePath)
    {
        var fullPath = ResolveFullPath(relativePath);
        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public bool FileExists(string relativePath)
    {
        var fullPath = ResolveFullPath(relativePath);
        return File.Exists(fullPath);
    }

    public void DeleteFile(string relativePath)
    {
        var fullPath = ResolveFullPath(relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);
    }

    /// <summary>
    /// Resolves a relative path like "uploads/abc.pdf" to its absolute path on disk.
    /// </summary>
    private string ResolveFullPath(string relativePath)
    {
        // The relative path stored in DB is "uploads/filename.ext"
        // Extract just the filename part and combine with base path
        var fileName = Path.GetFileName(relativePath);
        return Path.Combine(_basePath, fileName);
    }
}
