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
	private readonly ApplicationDbContext _context;

	public ModuleRepository(ApplicationDbContext context) : base(context)
	{
		this._context = context;
	}

	public async Task<Module?> GetModuleByIdAsync(int moduleId, bool trackChanges = false)
	{
		return await FindByCondition(m => m.Id == moduleId, trackChanges)
            .Include(m => m.Course)
			.ThenInclude(c => c.Modules)
			.Include(m => m.Activities)
			.ThenInclude(a => a.ActivityType)
			.FirstOrDefaultAsync();
	}

    public async Task<Module?> GetModuleWithActivitiesAsync(int moduleId, bool trackChanges = false)
    {
        var query = _context.Modules
            .Include(m => m.Activities)
            .ThenInclude(a => a.ActivityType)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(m => m.Id == moduleId);
    }

    public async Task<IEnumerable<Module>> GetByCourseIdAsync(int courseId, bool trackChanges = false)
    {
        var query = _context.Modules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.StartDate)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

	public async Task<Module?> GetModuleWithCourseAsync(int moduleId, bool trackChanges = false)
	{
		var query = _context.Modules
			.Include(m => m.Course)
			.AsQueryable();

		if (!trackChanges)
			query = query.AsNoTracking();

		return await query.FirstOrDefaultAsync(m => m.Id == moduleId);
	}
}