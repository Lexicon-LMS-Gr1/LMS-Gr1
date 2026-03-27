using LMS.Shared.DTOs.Module;
using LMS.Shared.DTOs.Activity;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<ModuleDto?> GetModuleByIdAsync(int id);
    Task<ModuleDto> CreateModuleAsync(ModuleCreateDto moduleDto);
    Task<ModuleDto> UpdateModuleAsync(ModuleUpdateDto moduleDto);
    Task<bool> DeleteModuleAsync(int id);

	Task<IEnumerable<ActivityDto>> GetActivitiesAsync(int moduleId);
}
