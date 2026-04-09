public class StudentAssignmentDto
{
    public int ActivityId { get; set; }
    public string Name { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsLate { get; set; }
    public bool IsSubmitted { get; set; }
    public int? SubmissionId { get; set; }
    public bool HasFeedback { get; set; }
    public string? Feedback { get; set; }
    public DateTime? FeedbackGivenAt { get; set; }
    public string? FeedbackGivenByTeacherName { get; set; }

}
