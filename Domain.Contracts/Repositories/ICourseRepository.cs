using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Repositories;

public interface ICourseRepository: IRepositoryBase<Course>
{
	  Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false);
    Task<Course?> GetCourseForUserAsync(string userId);
    Task<IEnumerable<ApplicationUser>> GetParticipantsForUserCourseAsync(string userId);
	  Task<IEnumerable<Course>> GetCoursesForListAsync(bool trackChanges = false);
}
