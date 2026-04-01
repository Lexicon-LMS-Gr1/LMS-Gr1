namespace Domain.Contracts.Services;

/// <summary>
/// Abstraction for file storage operations.
/// Keeps the business layer free from ASP.NET Core / web-host dependencies.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Save a file and return its relative storage path (e.g. "uploads/abc.pdf").</summary>
    Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken ct = default);

    /// <summary>Open a read-only stream for a previously stored file.</summary>
    Stream OpenReadStream(string relativePath);

    /// <summary>Check whether a stored file exists on disk.</summary>
    bool FileExists(string relativePath);

    /// <summary>Permanently delete a stored file from disk.</summary>
    void DeleteFile(string relativePath);
}
