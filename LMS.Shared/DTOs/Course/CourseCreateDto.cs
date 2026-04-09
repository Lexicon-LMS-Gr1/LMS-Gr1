using LMS.Shared.DTOs.Module;

namespace LMS.Shared.DTOs.Course;

public class CourseCreateDto : CourseBaseDto
{
    public List<ModuleCreateDto> Modules { get; set; } = [];
}
