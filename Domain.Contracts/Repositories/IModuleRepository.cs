using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Repositories;

public interface IModuleRepository : IRepositoryBase<Module>
{
	Task<Module?> GetModuleByIdAsync(int moduleId, bool trackChanges = false);
}
