namespace LMS.Shared.DTOs.Submission;

public class SubmissionDto
{
	public int Id { get; set; }
	public int ActivityId { get; set; }
	public string StudentId { get; set; }
	public string FileName { get; set; }
	public string Comment { get; set; }
	public DateTime SubmittedAt { get; set; }



	public string? Feedback { get; set; }
	public DateTime? FeedbackGivenAt { get; set; }

	public string? FeedbackGivenByTeacherId { get; set; }
	public string? FeedbackGivenByTeacherName { get; set; }
}
