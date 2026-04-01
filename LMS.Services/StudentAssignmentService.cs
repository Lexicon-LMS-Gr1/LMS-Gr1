using Domain.Contracts.Repositories;
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
                    IsSubmitted = submission != null,
                    IsLate = submission == null && now > a.DueDate
                };
            }).ToList();
        }

        public async Task<List<StudentAssignmentDto>> GetUpcomingAssignmentsAsync(string studentId)
        {
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
