using Domain.Contracts.Repositories;
using Domain.Models.Exceptions;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services
{
    public class StudentAssignmentService : IStudentAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentAssignmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<StudentAssignmentDto>> GetStudentAssignmentsAsync(string studentId, int courseId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                throw new BadRequestException("Elev-id saknas.", "Valideringsfel");

            if (courseId <= 0)
                throw new BadRequestException("Ogiltigt kurs-id.", "Valideringsfel");

            var activities = await _unitOfWork.ActivityRepository
                .GetSubmissionActivitiesForCourseAsync(courseId);

            var submissions = await _unitOfWork.SubmissionRepository
                .GetByStudentIdAsync(studentId);

            var now = DateTime.UtcNow;

            return activities.Select(a =>
            {
                var submission = submissions.FirstOrDefault(s => s.ActivityId == a.Id);

                return new StudentAssignmentDto
                {
                    ActivityId = a.Id,
                    Name = a.Name,
                    DueDate = a.DueDate,
                    ActivityStartDate = a.StartTime,
                    IsSubmitted = submission != null,
                    IsLate = submission == null && now > a.DueDate,

                    SubmissionId = submission?.Id,
                    HasFeedback = !string.IsNullOrWhiteSpace(submission?.Feedback),
                    Feedback = submission?.Feedback,
                    FeedbackGivenAt = submission?.FeedbackGivenAt,
                    FeedbackGivenByTeacherName = submission?.FeedbackGivenByTeacher != null
            ? $"{submission.FeedbackGivenByTeacher.FirstName} {submission.FeedbackGivenByTeacher.LastName}"
            : null
                };
            }).ToList();
        }

        public async Task<List<StudentAssignmentDto>> GetUpcomingAssignmentsAsync(string studentId)
        {
            if (string.IsNullOrWhiteSpace(studentId))
                throw new BadRequestException("Elev-id saknas.", "Valideringsfel");

            var course = await _unitOfWork.CourseRepository.GetCourseForUserAsync(studentId);
            if (course == null)
                return new List<StudentAssignmentDto>();

            var activities = await _unitOfWork.ActivityRepository
                .GetSubmissionActivitiesForCourseAsync(course.Id);

            var submissions = await _unitOfWork.SubmissionRepository
                .GetByStudentIdAsync(studentId);

            var now = DateTime.UtcNow;

            return activities
                .Where(a => a.DueDate > now)
                .OrderBy(a => a.DueDate)
                .Take(5)
                .Select(a =>
                {
                    var submission = submissions.FirstOrDefault(s => s.ActivityId == a.Id);

                    return new StudentAssignmentDto
                    {
                        ActivityId = a.Id,
                        Name = a.Name,
                        DueDate = a.DueDate,
                        IsSubmitted = submission != null,
                        IsLate = submission == null && now > a.DueDate
                    };
                })
                .ToList();
        }



    }

}
