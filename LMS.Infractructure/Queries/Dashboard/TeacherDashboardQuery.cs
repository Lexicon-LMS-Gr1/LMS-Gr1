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
        var courses = await _context.Courses
            .AsNoTracking()
            .Include(c => c.Modules)
            .Include(c => c.Students)
            .ToListAsync();

        var recent = await _context.Activities
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Include(a => a.Module)
                .ThenInclude(m => m.Course)
            .OrderByDescending(a => a.StartTime)
            .Take(10)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var upcoming = await _context.Activities
            .AsNoTracking()
            .Include(a => a.ActivityType)
            .Include(a => a.Module)
                .ThenInclude(m => m.Course)
            .Where(a => a.StartTime >= now)
            .OrderBy(a => a.StartTime)
            .Take(10)
            .ToListAsync();

        return new TeacherDashboardDto
        {
            ActiveCourses = courses
                .Where(c => c.StartDate <= now && c.EndDate >= now)
                .Select(c => new TeacherCourseSummaryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    StartDate = c.StartDate,
                    EndDate = c.EndDate,
                    ModuleCount = c.Modules.Count,
                    StudentCount = c.Students.Count
                })
                .ToList(),

            RecentActivities = recent.Select(a => new RecentActivityDto
            {
                Id = a.Id,
                Type = a.ActivityType.Name,
                Description = $"{a.Name} – {a.Module.Course.Name}",
                Timestamp = a.StartTime
            }).ToList(),

            UpcomingActivities = upcoming.Select(a => new UpcomingActivityDto
            {
                Id = a.Id,
                Type = a.ActivityType.Name,
                Name = a.Name,
                CourseName = a.Module.Course.Name,
                StartTime = a.StartTime
            }).ToList()
        };
    }
}
