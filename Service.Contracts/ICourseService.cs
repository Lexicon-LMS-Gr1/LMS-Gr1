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
<<<<<<< HEAD

        Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto);
=======
        Task<IEnumerable<ParticipantDto>> GetParticipantsForUserCourseAsync(string userId);
>>>>>>> 4162c50 (Backend is done, fontend is partially done)
    }

}
