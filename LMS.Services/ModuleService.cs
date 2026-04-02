using Domain.Contracts.Repositories;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

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

		if (module == null)
			return Enumerable.Empty<ActivityDto>();

		return module.Activities.Select(a => new ActivityDto {
			Id = a.Id,
			Name = a.Name,
			Description = a.Description,
			StartTime = a.StartTime,
			EndTime = a.EndTime,
			DueDate = a.DueDate,
			ActivityTypeName = a.ActivityType.Name
		});
	}


    //TODO: Skapar modul utan att koppla den till en kurs, används inte. Tas Bort?
    public Task<ModuleDto> CreateModuleAsync(ModuleCreateDto moduleDto)
	{
		throw new NotImplementedException();
	}

    public async Task<ModuleDto> CreateModuleAsync(int courseId, ModuleCreateDto moduleDto)
    {
        if (moduleDto is null)
            throw new ArgumentNullException(nameof(moduleDto));

        if (string.IsNullOrWhiteSpace(moduleDto.Name))
            throw new ArgumentException("Modulnamn saknas.");

        if (string.IsNullOrWhiteSpace(moduleDto.Description))
            throw new ArgumentException("Modulbeskrivning saknas.");

        if (moduleDto.StartDate > moduleDto.EndDate)
            throw new ArgumentException("Modulens startdatum kan inte ligga efter slutdatum.");

        var course = await _unitOfWork.CourseRepository.GetCourseById(courseId);

        if (course is null)
            throw new KeyNotFoundException($"Kurs med id {courseId} hittades inte.");

        if (moduleDto.StartDate < course.StartDate || moduleDto.EndDate > course.EndDate)
            throw new ArgumentException("Modulen ligger utanför kursens datumintervall.");

        bool overlaps = course.Modules.Any(m =>
            moduleDto.StartDate <= m.EndDate && moduleDto.EndDate >= m.StartDate);

        if (overlaps)
            throw new ArgumentException("Modulen överlappar en annan modul i kursen.");

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

    public async Task<bool> DeleteModuleAsync(int id)
    {
        var module = await _unitOfWork.ModuleRepository.GetModuleByIdAsync(id, trackChanges: true);

        if (module == null)
            return false;

        if (module.Activities != null && module.Activities.Any())
        {
            foreach (var activity in module.Activities.ToList())
            {
                _unitOfWork.ActivityRepository.Delete(activity);
            }
        }

        _unitOfWork.ModuleRepository.Delete(module);

        await _unitOfWork.CompleteAsync();

        return true;
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

    public async Task<ModuleDto?> UpdateModuleAsync(ModuleUpdateDto dto)
    {
        var module = await _unitOfWork.ModuleRepository.GetModuleByIdAsync(dto.Id, trackChanges: true);

        if (module == null)
            return null;

        module.Name = dto.Name;
        module.Description = dto.Description;
        module.StartDate = dto.StartDate;
        module.EndDate = dto.EndDate;
        module.CourseId = dto.CourseId;

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
