using Domain.Models.Entities;

public class Submission
{
    public int Id { get; set; }
    public int ActivityId { get; set; }
    public string StudentId { get; set; }
    public string FilePath { get; set; }
    public string FileName { get; set; }
    public string Comment { get; set; }
    public DateTime SubmittedAt { get; set; }

    public Activity Activity { get; set; }
    public ApplicationUser Student { get; set; }



	public string? Feedback { get; set; }
    public DateTime? FeedbackGivenAt { get; set; }

	public string? FeedbackGivenByTeacherId { get; set; }
	public ApplicationUser? FeedbackGivenByTeacher { get; set; }
}
