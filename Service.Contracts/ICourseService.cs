using LMS.Shared.DTOs.Course;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface ICourseService
    {
        Task<CourseDto?> GetCourseForUserAsync(string userId);
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();


		Task<IEnumerable<CourseListDto>> GetAllCoursesListAsync();
        Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto);
        Task<IEnumerable<ParticipantDto>> GetParticipantsForUserCourseAsync(string userId);


        Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
	}

}
