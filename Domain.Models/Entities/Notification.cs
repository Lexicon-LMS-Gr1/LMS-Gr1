using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Entities;

public class Notification
{
	public int Id { get; set; }

	// Mottagare av notisen
	public required string UserId { get; set; }

	public NotificationType Type { get; set; }

	public DateTime CreatedAt { get; set; }
	public bool IsRead { get; set; }

	// Vem skapade notisen
	public string? ActorUserId { get; set; }
	public string? ActorName { get; set; }

	// Kontext
	public int? CourseId { get; set; }
	public string? CourseName { get; set; }

	public int? ModuleId { get; set; }
	public string? ModuleName { get; set; }

	public int? ActivityId { get; set; }
	public string? ActivityName { get; set; }

	public int? DocumentId { get; set; }
	public string? DocumentName { get; set; }

	public int? SubmissionId { get; set; }

	public string? Message { get; set; }
}