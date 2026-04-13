using Domain.Contracts.Repositories;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Module;
using Service.Contracts;
using Domain.Models.Exceptions;

namespace LMS.Services;

public class ModuleService : IModuleService
{
    private readonly IUnitOfWork _unitOfWork;

    public ModuleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesAsync(int moduleId)
    {
        var module = await _unitOfWork.ModuleRepository.GetModuleByIdAsync(moduleId);

        //if (module == null)
        //    return Enumerable.Empty<ActivityDto>();

        if (module == null)
            throw new NotFoundException($"Modul med id {moduleId} hittades inte.");

        return module.Activities.Select(a => new ActivityDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            DueDate = a.DueDate,
            ActivityTypeName = a.ActivityType.Name
        });
    }

    public async Task<ModuleDto> CreateModuleAsync(int courseId, ModuleCreateDto moduleDto)
    {
        if (moduleDto is null)
            throw new BadRequestException("Moduldata saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(moduleDto.Name))
            throw new BadRequestException("Modulnamn saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(moduleDto.Description))
            throw new BadRequestException("Modulbeskrivning saknas.", "Valideringsfel");

        if (moduleDto.StartDate > moduleDto.EndDate)
            throw new BadRequestException("Startdatum får inte vara senare än slutdatum.", "Valideringsfel");

        var course = await _unitOfWork.CourseRepository.GetCourseById(courseId);

        if (course is null)
            throw new NotFoundException($"Kurs med id \"{courseId}\" hittades inte.");

        if (moduleDto.StartDate < course.StartDate || moduleDto.EndDate > course.EndDate)
            throw new BadRequestException($"Modul \"{moduleDto.Name}\" ligger utanför kursens datumintervall.", "Valideringsfel");

        bool overlaps = course.Modules.Any(m =>
            moduleDto.StartDate <= m.EndDate && moduleDto.EndDate >= m.StartDate);

        if (overlaps)
            throw new BadRequestException("Modulen överlappar en annan modul i kursen.", "Valideringsfel");

        var module = new Domain.Models.Entities.Module
        {
            Name = moduleDto.Name.Trim(),
            Description = moduleDto.Description.Trim(),
            StartDate = moduleDto.StartDate,
            EndDate = moduleDto.EndDate,
            CourseId = courseId,
            Course = null!
        };

        _unitOfWork.ModuleRepository.Create(module);
        await _unitOfWork.CompleteAsync();

        return new ModuleDto
        {
            Id = module.Id,
            Name = module.Name,
            Description = module.Description,
            StartDate = module.StartDate,
            EndDate = module.EndDate,
            Activities = new List<ActivityDto>()
        };
    }

    public async Task DeleteModuleAsync(int id)
    {
        var module = await _unitOfWork.ModuleRepository.GetModuleByIdAsync(id, trackChanges: true);

        if (module == null)
            throw new NotFoundException($"Modul med id {id} hittades inte.");

        if (module.Activities != null && module.Activities.Any())
        {
            foreach (var activity in module.Activities.ToList())
            {
                _unitOfWork.ActivityRepository.Delete(activity);
            }
        }

        _unitOfWork.ModuleRepository.Delete(module);

        await _unitOfWork.CompleteAsync();
    }

    public Task<IEnumerable<ModuleDto>> GetAllModulesAsync()
    {
        throw new NotImplementedException();
    }

    public Task<ModuleDto?> GetModuleByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId)
    {
        var modules = await _unitOfWork.ModuleRepository.GetByCourseIdAsync(courseId);

        return modules.Select(m => new ModuleDto
        {
            Id = m.Id,
            Name = m.Name,
            Description = m.Description,
            StartDate = m.StartDate,
            EndDate = m.EndDate,
            Activities = new List<ActivityDto>()
        });
    }

    public async Task<ModuleDto> UpdateModuleAsync(ModuleUpdateDto moduleUpdateDto)
    {
        var module = await _unitOfWork.ModuleRepository.GetModuleByIdAsync(moduleUpdateDto.Id, trackChanges: true);

        if (module == null)
            throw new NotFoundException($"Modul med id {moduleUpdateDto.Id} hittades inte.");

        if (string.IsNullOrWhiteSpace(moduleUpdateDto.Name))
            throw new BadRequestException("Modulnamn saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(moduleUpdateDto.Description))
            throw new BadRequestException("Modulbeskrivning saknas.", "Valideringsfel");

        if (moduleUpdateDto.StartDate > moduleUpdateDto.EndDate)
            throw new BadRequestException("Startdatum får inte vara senare än slutdatum.", "Valideringsfel");

        if (moduleUpdateDto.StartDate < module.Course.StartDate || moduleUpdateDto.EndDate > module.Course.EndDate)
            throw new BadRequestException($"Modul \"{module.Name}\" ligger utanför kursens datumintervall.", "Valideringsfel");

        bool overlaps = module.Course.Modules.Any(m =>
            m.Id != module.Id &&
            moduleUpdateDto.StartDate <= m.EndDate && moduleUpdateDto.EndDate >= m.StartDate);

        if (overlaps)
            throw new BadRequestException("Modulen överlappar en annan modul i kursen.", "Valideringsfel");

        module.Name = moduleUpdateDto.Name.Trim();
        module.Description = moduleUpdateDto.Description.Trim();
        module.StartDate = moduleUpdateDto.StartDate;
        module.EndDate = moduleUpdateDto.EndDate;
        module.CourseId = moduleUpdateDto.CourseId;

        await _unitOfWork.CompleteAsync();

        return new ModuleDto
        {
            Id = module.Id,
            Name = module.Name,
            Description = module.Description,
            StartDate = module.StartDate,
            EndDate = module.EndDate
        };
    }
}
