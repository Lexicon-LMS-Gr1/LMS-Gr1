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

        public async Task<IEnumerable<Submission>> GetAllAsync()
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Activity)
                    .ThenInclude(a => a.Module)
                        .ThenInclude(m => m.Course)
                .ToListAsync();
        }

        public async Task<Submission?> GetByIdAsync(int id)
		{
			return await _context.Submissions.Include(s => s.FeedbackGivenByTeacher).FirstOrDefaultAsync(s => s.Id == id);
		}

		public async Task<IEnumerable<Submission>> GetByCourseIdAsync(int courseId, bool trackChanges = false)
		{
			var query = _context.Submissions
				.Include(s => s.FeedbackGivenByTeacher)
				.Where(s => s.Activity.Module.CourseId == courseId)
				.AsQueryable();

			if (!trackChanges)
				query = query.AsNoTracking();

			return await query.ToListAsync();
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

        public async Task<IEnumerable<Submission>> GetByActivityIdAsync(int activityId, bool trackChanges = false)
        {
            var query = _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Activity)
                    .ThenInclude(a => a.Module)
                        .ThenInclude(m => m.Course)
                .Where(s => s.ActivityId == activityId)
                .AsQueryable();

            if (!trackChanges)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }

    }

}
