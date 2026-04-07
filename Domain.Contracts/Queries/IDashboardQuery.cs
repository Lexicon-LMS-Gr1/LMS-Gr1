using LMS.Shared.DTOs.StudentDashboard;

namespace LMS.Infrastructure.Queries.Dashboard;

public interface IDashboardQuery
{
    Task<List<WeeklyActivityDto>> GetWeeklyActivitiesAsync(
        string userId, DateTime startOfWeek, DateTime endOfWeek);
}
