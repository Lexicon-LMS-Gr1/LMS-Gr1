namespace Domain.Contracts.Services;

/// <summary>
/// Abstraction for file storage operations.
/// Keeps the business layer free from ASP.NET Core / web-host dependencies.
/// </summary>
public interface IFileStorageService
{
    //Save a file and return its relative storage path (e.g. "uploads/abc.pdf")
    Task<string> SaveFileAsync(Stream stream, string fileName, CancellationToken ct = default);

    //Open a read-only stream for a previously stored file
    Stream OpenReadStream(string relativePath);

    // Check whether a stored file exists on disk
    bool FileExists(string relativePath);

    //Permanently delete a stored file from disk
    void DeleteFile(string relativePath);
}
