using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Activity
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime? DueDate { get; set; }

        public string ActivityTypeName { get; set; } = "";
    }
}
