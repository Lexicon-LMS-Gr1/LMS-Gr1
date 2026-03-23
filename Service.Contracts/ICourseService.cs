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

        Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto);
    }

}
