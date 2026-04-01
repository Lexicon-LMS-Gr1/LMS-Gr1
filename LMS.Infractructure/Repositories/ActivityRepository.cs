using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly ApplicationDbContext _context;

    public ActivityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync(bool trackChanges = false)
    {
        var query = _context.Activities
            .Include(a => a.ActivityType)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetByModuleIdAsync(int moduleId, bool trackChanges = false)
    {
        var query = _context.Activities
            .Include(a => a.ActivityType)
            .Where(a => a.ModuleId == moduleId)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<Activity?> GetByIdAsync(int id, bool trackChanges = false)
    {
        var query = _context.Activities
            .Include(a => a.ActivityType)
            .AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query.FirstOrDefaultAsync(a => a.Id == id);
    }

    public void Create(Activity activity)
    {
        _context.Activities.Add(activity);
    }

    public void Update(Activity activity)
    {
        _context.Activities.Update(activity);
    }

    public void Delete(Activity activity)
    {
        _context.Activities.Remove(activity);
    }

    public async Task<bool> ActivityTypeExistsAsync(int activityTypeId)
    {
        return await _context.ActivityTypes.AnyAsync(at => at.Id == activityTypeId);
    }

    public async Task<IEnumerable<ActivityType>> GetAllActivityTypesAsync(bool trackChanges = false)
    {
        var query = _context.ActivityTypes.AsQueryable();

        if (!trackChanges)
            query = query.AsNoTracking();

        return await query
            .OrderBy(at => at.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetSubmissionActivitiesForCourseAsync(int courseId)
    {
        return await _context.Activities
            .Include(a => a.ActivityType)
            .Include(a => a.Module)
            .Where(a =>
                a.ActivityType.Name == "Assignment" &&
                a.Module.CourseId == courseId)
            .AsNoTracking()
            .ToListAsync();
    }

	public async Task<IEnumerable<Activity>> GetByCourseIdAsync(int courseId, bool trackChanges = false)
	{
        var query = _context.Activities
            .Include(a => a.ActivityType)
            .Include(a => a.Module)
            .Where(a => a.Module.CourseId == courseId)
            .AsQueryable();
        if (!trackChanges)
            query = query.AsNoTracking();
        return await query.OrderBy(a => a.StartTime).ToListAsync();
	}
}