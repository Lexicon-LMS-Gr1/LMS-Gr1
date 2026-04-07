using LMS.Shared.DTOs.StudentDashboard;
using Microsoft.EntityFrameworkCore;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Queries.Dashboard;

public class DashboardQuery : IDashboardQuery
{
    private readonly ApplicationDbContext _context;

    public DashboardQuery(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WeeklyActivityDto>> GetWeeklyActivitiesAsync(
        string userId, DateTime startOfWeek, DateTime endOfWeek)
    {
        var courseId = await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.CourseId)
            .FirstOrDefaultAsync();

        if (courseId is null)
            return new List<WeeklyActivityDto>();

        return await _context.Activities
            .AsNoTracking()
            .Where(a =>
                a.Module.CourseId == courseId &&
                a.StartTime >= startOfWeek &&
                a.StartTime <= endOfWeek)
            .OrderBy(a => a.StartTime)
            .Select(a => new WeeklyActivityDto
            {
                ModuleName = a.Module.Name,
                ActivityType = a.ActivityType.Name,
                Title = a.Name,
                Start = a.StartTime,
                End = a.EndTime
            })
            .ToListAsync();
    }
}
