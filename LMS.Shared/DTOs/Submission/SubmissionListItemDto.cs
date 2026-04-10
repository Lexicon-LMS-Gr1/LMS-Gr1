using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.Submission
{
    public class SubmissionListItemDto
    {
        public int SubmissionId { get; set; }

        public string StudentId { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public string StudentEmail { get; set; } = null!;

        public string CourseName { get; set; } = null!;
        public string ModuleName { get; set; } = null!;
        public string ActivityName { get; set; } = null!;
        public int ActivityId { get; set; }

        public DateTime SubmittedAt { get; set; }
        public string FileName { get; set; }

        public bool HasFeedback { get; set; }
        public DateTime? FeedbackGivenAt { get; set; }
    }

}
