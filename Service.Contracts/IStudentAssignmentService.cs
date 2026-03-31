using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface IStudentAssignmentService
    {
        Task<List<StudentAssignmentDto>> GetStudentAssignmentsAsync(string studentId, int courseId);
    }
}
