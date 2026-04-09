using Domain.Contracts.Queries;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.TeacherDashboard;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Queries.TeacherDashboard;

public class TeacherDashboardQuery : ITeacherDashboardQuery
{
    private readonly ApplicationDbContext _context;

    public TeacherDashboardQuery(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;

        // Active courses
        var activeCourses = await _context.Courses
            .AsNoTracking()
            .Where(c => c.StartDate <= now && c.EndDate >= now)
            .Select(c => new TeacherCourseSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                ModuleCount = c.Modules.Count,
                StudentCount = c.Users.Count
            })
            .ToListAsync();

        // Recent activities
        var recent = await _context.Activities
            .AsNoTracking()
            .OrderByDescending(a => a.StartTime)
            .Take(10)
            .Select(a => new RecentActivityDto
            {
                Id = a.Id,
                Type = a.ActivityType.Name,
                Description = a.Name + " – " + a.Module.Course.Name,
                Timestamp = a.StartTime
            })
            .ToListAsync();

        // Upcoming activities
        var upcoming = await _context.Activities
            .AsNoTracking()
            .Where(a => a.StartTime >= now)
            .OrderBy(a => a.StartTime)
            .Take(10)
            .Select(a => new UpcomingActivityDto
            {
                Id = a.Id,
                Type = a.ActivityType.Name,
                Name = a.Name,
                CourseName = a.Module.Course.Name,
                StartTime = a.StartTime
            })
            .ToListAsync();

        return new TeacherDashboardDto
        {
            ActiveCourses = activeCourses,
            RecentActivities = recent,
            UpcomingActivities = upcoming
        };
    }

}
