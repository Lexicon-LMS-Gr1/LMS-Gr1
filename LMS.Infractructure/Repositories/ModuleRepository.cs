using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infractructure.Repositories;

public class ModuleRepository : RepositoryBase<Module>, IModuleRepository
{
	private readonly ApplicationDbContext context;

	public ModuleRepository(ApplicationDbContext context) : base(context)
	{
		this.context = context;
	}

	public async Task<Module?> GetModuleByIdAsync(int moduleId, bool trackChanges = false)
	{
		return await FindByCondition(m => m.Id == moduleId, trackChanges)
			.Include(m => m.Activities)
			.ThenInclude(a => a.ActivityType)
			.FirstOrDefaultAsync();
	}
}