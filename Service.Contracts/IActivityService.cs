using LMS.Shared.DTOs.Activity;

namespace Service.Contracts;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync();
    Task<IEnumerable<ActivityDto>> GetActivitiesByModuleIdAsync(int moduleId);
    Task<ActivityDto?> GetActivityByIdAsync(int id);
    Task<ActivityDto> CreateActivityAsync(int moduleId, ActivityCreateDto activityDto);
    //Task<ActivityDto> UpdateActivityAsync(ActivityUpdateDto_Old activityDto);
    Task<bool> DeleteActivityAsync(int id);
    Task<IEnumerable<ActivityTypeDto>> GetAllActivityTypesAsync();
	Task<ActivityDto> UpdateActivityAsync(ActivityUpdateDto dto);
}
