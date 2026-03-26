using LMS.Shared.DTOs.Activity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Module
{
    public class ModuleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<ActivityDto> Activities { get; set; } = new();
    }
}
