using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Course;

namespace Service.Contracts;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync();
    Task<IEnumerable<ActivityDto>> GetActivitiesByModuleIdAsync(int moduleId);
    Task<ActivityDto?> GetActivityByIdAsync(int id);
    Task<ActivityDto> CreateActivityAsync(ActivityCreateDto activityDto);
    Task<ActivityDto> UpdateActivityAsync(ActivityUpdateDto activityDto);
    Task<bool> DeleteActivityAsync(int id);
}
