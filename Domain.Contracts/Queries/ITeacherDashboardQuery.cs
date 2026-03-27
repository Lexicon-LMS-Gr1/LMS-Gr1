using LMS.Shared.DTOs.TeacherDashboard;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Queries
{
    public interface ITeacherDashboardQuery
    {
        Task<TeacherDashboardDto> GetDashboardAsync();
    }
}
