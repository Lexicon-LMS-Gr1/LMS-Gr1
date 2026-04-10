using Domain.Contracts.Services;
using Microsoft.Extensions.Configuration;

namespace LMS.Infractructure.Services;

/// <summary>
/// Local file-system storage implementation.
/// Reads the base path from configuration ("FileStorage:BasePath"), defaults to "uploads".
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        var configuredPath = configuration["FileStorage:BasePath"];

        _basePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "uploads")
            : Path.IsPathRooted(configuredPath)
                ? configuredPath
                : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);

        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream stream, string relativePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        Directory.CreateDirectory(directory);

        using var outputStream = new FileStream(fullPath, FileMode.Create);
        await stream.CopyToAsync(outputStream, ct);

        return relativePath;
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

    private string ResolveFullPath(string relativePath)
    {
        return Path.Combine(_basePath, relativePath);
    }
}
