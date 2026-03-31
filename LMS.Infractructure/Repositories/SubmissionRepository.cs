using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infractructure.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Submission>> GetByStudentIdAsync(string studentId, bool trackChanges = false)
        {
            var query = _context.Submissions
                .Where(s => s.StudentId == studentId)
                .AsQueryable();

            if (!trackChanges)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }
    }

}
