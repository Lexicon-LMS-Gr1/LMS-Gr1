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


	public Task<ModuleDto> CreateModuleAsync(ModuleCreateDto moduleDto)
	{
		throw new NotImplementedException();
	}

	public Task<bool> DeleteModuleAsync(int id)
	{
		throw new NotImplementedException();
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

    public Task<ModuleDto> UpdateModuleAsync(ModuleUpdateDto moduleDto)
	{
		throw new NotImplementedException();
	}
}
