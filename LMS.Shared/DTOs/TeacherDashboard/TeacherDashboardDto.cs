using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.TeacherDashboard
{
    public class TeacherDashboardDto
    {
        public IEnumerable<TeacherCourseSummaryDto> ActiveCourses { get; set; } = new List<TeacherCourseSummaryDto>();
        public IEnumerable<RecentActivityDto> RecentActivities { get; set; } = new List<RecentActivityDto>();
        public IEnumerable<UpcomingActivityDto> UpcomingActivities { get; set; } = new List<UpcomingActivityDto>();
    }

    public class TeacherCourseSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int StudentCount { get; set; }
        public int ModuleCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class RecentActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;      // "Föreläsning", "Inlämning", etc (ActivityType.Name)
        public string Description { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }

    public class UpcomingActivityDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CourseName { get; set; } = null!;
        public DateTime StartTime { get; set; }
    }

    public class StudentListDto
    {
        public string Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string CourseName { get; set; } = string.Empty;
        public int? CourseId { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
