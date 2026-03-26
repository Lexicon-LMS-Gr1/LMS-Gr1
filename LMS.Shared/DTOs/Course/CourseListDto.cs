namespace LMS.Shared.DTOs.Course;

/// <summary>
/// Lightweight DTO for listing courses (without full module/activity tree)
/// Used in Teacher dashboard and course selection lists
/// </summary>
public class CourseListDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public int StudentCount { get; set; }
    
    public int ModuleCount { get; set; }
    
    public bool IsActive => DateTime.Now >= StartDate && DateTime.Now <= EndDate;
    
    public string Status => DateTime.Now < StartDate ? "Upcoming" : 
                           DateTime.Now > EndDate ? "Completed" : "Active";
}
