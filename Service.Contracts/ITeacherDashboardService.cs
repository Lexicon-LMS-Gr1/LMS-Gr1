using LMS.Shared.DTOs.TeacherDashboard;

namespace Service.Contracts;

public interface ITeacherDashboardService
{
    Task<TeacherDashboardDto> GetDashboardAsync();
}
