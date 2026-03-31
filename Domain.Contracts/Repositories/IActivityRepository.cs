using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface IActivityRepository : IRepositoryBase<Activity>
{
    Task<IEnumerable<Activity>> GetAllAsync(bool trackChanges = false);
    Task<IEnumerable<Activity>> GetByModuleIdAsync(int moduleId, bool trackChanges = false);
    Task<Activity?> GetByIdAsync(int id, bool trackChanges = false);
    Task<bool> ActivityTypeExistsAsync(int activityTypeId);
    Task<IEnumerable<ActivityType>> GetAllActivityTypesAsync(bool trackChanges = false);
    Task<IEnumerable<Activity>> GetSubmissionActivitiesForCourseAsync(int courseId);
}
