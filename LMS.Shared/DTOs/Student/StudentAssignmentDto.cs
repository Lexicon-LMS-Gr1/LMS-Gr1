public class StudentAssignmentDto
{
    public int ActivityId { get; set; }
    public string Name { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsLate { get; set; }
    public bool IsSubmitted { get; set; }
}
