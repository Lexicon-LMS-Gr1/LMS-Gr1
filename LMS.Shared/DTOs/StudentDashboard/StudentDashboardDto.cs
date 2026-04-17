using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.StudentDashboard
{
    public class StudentDashboardDto
    {
        public List<WeeklyActivityDto> WeeklyActivities { get; set; } = new();
    }

    public class WeeklyActivityDto
    {
        public string ModuleName { get; set; } = "";
        public string ActivityTypeName { get; set; } = "";
        public string Title { get; set; } = "";
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
