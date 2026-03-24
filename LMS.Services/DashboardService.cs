using LMS.Shared.DTOs.StudentDashboard;
using Service.Contracts;

namespace LMS.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ICourseService _courseService;

        public DashboardService(ICourseService courseService)
        {
            _courseService = courseService;
        }

        public async Task<StudentDashboardDto> GetDashboardAsync(string userId)
        {
            var course = await _courseService.GetCourseForUserAsync(userId);

            if (course == null)
                return new StudentDashboardDto();

            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1);
            var endOfWeek = startOfWeek.AddDays(7);

            var weekly = new List<WeeklyActivityDto>();

            foreach (var module in course.Modules)
            {
                foreach (var activity in module.Activities)
                {
                    if (activity.StartTime >= startOfWeek && activity.StartTime <= endOfWeek)
                    {
                        weekly.Add(new WeeklyActivityDto
                        {
                            ModuleName = module.Name,
                            ActivityType = activity.ActivityTypeName,
                            Title = activity.Name,
                            Start = activity.StartTime,
                            End = activity.EndTime
                        });
                    }
                }
            }

            weekly = weekly.OrderBy(a => a.Start).ToList();

            return new StudentDashboardDto
            {
                WeeklyActivities = weekly
            };
        }
    }
}