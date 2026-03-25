using Domain.Contracts.Repositories;
using Domain.Models.Entities;
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

		public async Task<IEnumerable<CourseListDto>> GetAllCoursesListAsync()
		{
			var courses = await _unitOfWork.CourseRepository.GetCoursesForListAsync();

			return courses.Select(c => new CourseListDto {
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,
				StartDate = c.StartDate,
				EndDate = c.EndDate,
				StudentCount = c.Students.Count,
				ModuleCount = c.Modules.Count
			});
		}



		public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
		{
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();

			return courses.Select(c => new CourseDto {
				Id = c.Id,
				Name = c.Name,
				Description = c.Description,                
                StartDate = c.StartDate,
                EndDate = c.EndDate
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

        public async Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto)
        {
            if (string.IsNullOrWhiteSpace(courseCreateDto.Name))
                throw new ArgumentException("Course name is required.");

            if (courseCreateDto.StartDate > courseCreateDto.EndDate)
                throw new ArgumentException("Start date cannot be later than end date.");

            var course = new Course
            {
                Name = courseCreateDto.Name,
                Description = courseCreateDto.Description,
                StartDate = courseCreateDto.StartDate,
                EndDate = courseCreateDto.EndDate
            };

            _unitOfWork.CourseRepository.Create(course);
            await _unitOfWork.CompleteAsync();

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };
        }


        public async Task<CourseDto> UpdateCourseAsync(CourseUpdateDto courseUpdateDto)
        {
            var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseUpdateDto.Id);

            if (course == null)
                throw new Exception("Course not found");

            if (courseUpdateDto.EndDate < courseUpdateDto.StartDate)
                throw new Exception("End date must be after start date");

            course.Name = courseUpdateDto.Name;
            course.Description = courseUpdateDto.Description;
            course.StartDate = courseUpdateDto.StartDate;
            course.EndDate = courseUpdateDto.EndDate;

            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.CompleteAsync();

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };
        }



        public async Task<IEnumerable<ParticipantDto>> GetParticipantsForUserCourseAsync(string userId)
        {
            var users = await _unitOfWork.CourseRepository.GetParticipantsForUserCourseAsync(userId);

            return users.Select(u => new ParticipantDto
            {
                Id = u.Id,
                FullName = $"{u.FirstName} {u.LastName}",
                Email = u.Email!
            });
        }
    }
}