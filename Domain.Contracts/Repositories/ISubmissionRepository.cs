using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts.Repositories
{
    public interface ISubmissionRepository
    {
        Task<IEnumerable<Submission>> GetByStudentIdAsync(string studentId, bool trackChanges = false);
		Task<Submission?> GetByIdAsync(int id);

		Task<IEnumerable<Submission>> GetByCourseIdAsync(int courseId, bool trackChanges = false);
	}

}
