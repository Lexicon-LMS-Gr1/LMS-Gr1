using Domain.Models.Exceptions;
using LMS.Shared.DTOs.StudentDashboard;
using LMS.Infrastructure.Queries.Dashboard;
using Service.Contracts;

namespace LMS.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardQuery _dashboardQuery;

        public DashboardService(IDashboardQuery dashboardQuery)
        {
            _dashboardQuery = dashboardQuery;
        }

        public async Task<StudentDashboardDto> GetDashboardAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

            var (startOfWeek, endOfWeek) = GetCurrentWeek();

            var weekly = await _dashboardQuery.GetWeeklyActivitiesAsync(
                userId, startOfWeek, endOfWeek);

            return new StudentDashboardDto
            {
                WeeklyActivities = weekly
            };
        }

        private static (DateTime start, DateTime end) GetCurrentWeek()
        {
            var today = DateTime.Today;
            var diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
            if (diff < 0) diff += 7;

            var start = today.AddDays(-diff);
            var end = start.AddDays(7);

            return (start, end);
        }
    }
}