using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;

namespace LMS.Blazor.Client.Components.CourseOverview;

public static class CourseOverviewMapper
{
    public static CourseOverviewViewModel ToViewModel(this CourseDto dto)
    {
        return new CourseOverviewViewModel
        {
            Name = dto.Name,
            Description = dto.Description,
            DateRange = $"{dto.StartDate:yyyy-MM-dd} – {dto.EndDate:yyyy-MM-dd}",
            Progress = dto.Progress,
            Modules = dto.Modules.Select(m => m.ToViewModel()).ToList()
        };
    }

    public static ModuleViewModel ToViewModel(this ModuleDto dto)
    {
        return new ModuleViewModel
        {
            Name = dto.Name,
            Description = dto.Description,
            DateRange = $"{dto.StartDate:yyyy-MM-dd} – {dto.EndDate:yyyy-MM-dd}",
            Progress = 0,
            Activities = dto.Activities.Select(a => a.ToViewModel()).ToList()
        };
    }

    public static ActivityViewModel ToViewModel(this ActivityDto dto)
    {
        return new ActivityViewModel
        {
            Name = dto.Name,
            ActivityTypeName = dto.ActivityTypeName,
            DateRange = $"{dto.StartTime:yyyy-MM-dd HH:mm} – {dto.EndTime:HH:mm}",

        };
    }
}