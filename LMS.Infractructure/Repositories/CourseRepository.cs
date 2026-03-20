using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infractructure.Repositories;

//https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Infractructure/Repositories/EmployeeRepository.cs

public class CourseRepository : RepositoryBase<Course>, ICourseRepository
{

	public CourseRepository(ApplicationDbContext context) : base(context) { }


	public async Task<IEnumerable<Course>> GetAllAsync(bool trackChanges = false)
	{
		return await FindAll(trackChanges).ToListAsync();
	}


}
