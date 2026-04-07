using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Activity
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? DueDate { get; set; }
        public string DateRange => $"{StartTime:yyyy-MM-dd HH:mm} – {EndTime:HH:mm}";
        public int ActivityTypeId { get; set; }
        public string ActivityTypeName { get; set; } = string.Empty;
    }
}
