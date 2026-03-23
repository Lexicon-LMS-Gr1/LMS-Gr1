using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.Course;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;

namespace LMS.Services
{
	// https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Services/EmployeeService.cs
	public class CourseService : ICourseService
    {
		private readonly IUnitOfWork _unitOfWork;

		public CourseService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
		{
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();

			return courses.Select(c => new CourseDto {
				Id = c.Id,
				Name = c.Name,
				Description = c.Description
			});
		}
        public async Task<CourseDto?> GetCourseForUserAsync(string userId)
        {
            var course = await _unitOfWork.CourseRepository.GetCourseForUserAsync(userId);

            if (course == null)
                return null;

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate,
                Modules = course.Modules.Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    Description = m.Description,
                    StartDate = m.StartDate,
                    EndDate = m.EndDate,
                    Activities = m.Activities.Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        Name = a.Name,
                        Description = a.Description,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        DueDate = a.DueDate,
                        ActivityTypeName = a.ActivityType.Name
                    }).ToList()
                }).ToList()
            };
        }
    }
}