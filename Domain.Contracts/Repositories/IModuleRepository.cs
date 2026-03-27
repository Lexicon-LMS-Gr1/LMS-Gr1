using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Repositories;

public interface IModuleRepository : IRepositoryBase<Module>
{
	Task<Module?> GetModuleByIdAsync(int moduleId, bool trackChanges = false);
    Task<Module?> GetModuleWithActivitiesAsync(int moduleId, bool trackChanges = false);
    Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId, bool trackChanges = false);
}
