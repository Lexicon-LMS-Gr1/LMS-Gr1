using Domain.Contracts.Repositories;
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

	public Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId)
	{
		throw new NotImplementedException();
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
