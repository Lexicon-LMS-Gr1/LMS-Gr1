using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Course
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? DueDate { get; set; }
        public string DateRange => $"{StartTime:yyyy-MM-dd HH:mm} – {EndTime:HH:mm}";

        public string ActivityTypeName { get; set; } = "";
    }
}
