using Domain.Contracts.Queries;
using LMS.Infrastructure.Queries.TeacherDashboard;
using LMS.Shared.DTOs.TeacherDashboard;
using Service.Contracts;

namespace LMS.Services;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly ITeacherDashboardQuery _query;

    public TeacherDashboardService(ITeacherDashboardQuery query)
    {
        _query = query;
    }

    public Task<TeacherDashboardDto> GetDashboardAsync()
    {
        return _query.GetDashboardAsync();
    }
}
