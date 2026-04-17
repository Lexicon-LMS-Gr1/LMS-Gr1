using LMS.Shared.DTOs.Module;
using LMS.Shared.DTOs.Activity;

namespace Service.Contracts;

public interface IModuleService
{
    Task<IEnumerable<ModuleDto>> GetAllModulesAsync();
    Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId);
    Task<ModuleDto?> GetModuleByIdAsync(int id);
    Task<ModuleDto> UpdateModuleAsync(ModuleUpdateDto moduleDto);
    Task DeleteModuleAsync(int id);
	Task<IEnumerable<ActivityDto>> GetActivitiesAsync(int moduleId);
    Task<ModuleDto> CreateModuleAsync(int courseId, ModuleCreateDto moduleDto);
}
