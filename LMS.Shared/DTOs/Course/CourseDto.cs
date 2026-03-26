using LMS.Shared.DTOs.Module;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Course
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<ModuleDto> Modules { get; set; } = new();
        public int Progress { get; set; }
    }
}
