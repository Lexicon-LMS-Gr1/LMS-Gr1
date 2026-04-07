namespace Domain.Models.Entities;

/// <summary>
/// Represents a document uploaded to a Course, Module, or Activity.
/// Files are stored on the file system; only metadata is kept in the database.
/// Exactly one parent FK (CourseId, ModuleId, or ActivityId) must be set.
/// </summary>
public class Document
{
    public int Id { get; set; }

    /// <summary>Display name chosen by the uploader.</summary>
    public required string Name { get; set; }

    /// <summary>Optional description of the document.</summary>
    public string? Description { get; set; }

    /// <summary>UTC timestamp when the document was uploaded.</summary>
    public DateTime UploadTimestamp { get; set; }

    /// <summary>Relative path on the server file system (e.g. "uploads/abc123.pdf").</summary>
    public required string FilePath { get; set; }

    /// <summary>Original file name as uploaded by the user (e.g. "assignment.pdf").</summary>
    public required string FileName { get; set; }

    /// <summary>MIME content type (e.g. "application/pdf").</summary>
    public required string ContentType { get; set; }

    /// <summary>File size in bytes.</summary>
    public long FileSize { get; set; }

    // ── Uploader ──────────────────────────────────────────────
    public required string UploadedByUserId { get; set; }
    public ApplicationUser? UploadedBy { get; set; }

    // ── Parent entity (exactly ONE must be non-null) ─────────
    public int? CourseId { get; set; }
    public Course? Course { get; set; }

    public int? ModuleId { get; set; }
    public Module? Module { get; set; }

    public int? ActivityId { get; set; }
    public Activity? Activity { get; set; }
}
