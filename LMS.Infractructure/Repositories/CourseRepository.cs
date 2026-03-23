using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infractructure.Repositories;

//https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Infractructure/Repositories/EmployeeRepository.cs

public class CourseRepository : RepositoryBase<Course>, ICourseRepository
{
    private readonly ApplicationDbContext context;

    public CourseRepository(ApplicationDbContext context) : base(context) 
    {
        this.context = context;
    }


    public async Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false)
	{
		return await FindAll(trackChanges).ToListAsync();
	}

    public async Task<Course?> GetCourseForUserAsync(string userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.CourseId == null)
            return null;

        return await context.Courses
            .Include(c => c.Modules)
                .ThenInclude(m => m.Activities)
                    .ThenInclude(a => a.ActivityType)
            .FirstOrDefaultAsync(c => c.Id == user.CourseId);
    }
	public async Task<IEnumerable<Course>> GetAllWithStudentsAndModulesAsync(bool trackChanges = false)
	{
		return await context.Courses
			.Include(c => c.Students)
			.Include(c => c.Modules)
			.ToListAsync();
	}

    public async Task<IEnumerable<ApplicationUser>> GetParticipantsForUserCourseAsync(string userId)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.CourseId == null)
            return Enumerable.Empty<ApplicationUser>();

        return await context.Users
            .Where(u => u.CourseId == user.CourseId)
            .ToListAsync();
    }
}
