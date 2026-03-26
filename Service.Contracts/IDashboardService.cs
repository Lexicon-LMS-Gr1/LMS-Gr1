using LMS.Shared.DTOs.StudentDashboard;

namespace Service.Contracts
{
    public interface IDashboardService
    {
        Task<StudentDashboardDto> GetDashboardAsync(string userId);
    }

}
