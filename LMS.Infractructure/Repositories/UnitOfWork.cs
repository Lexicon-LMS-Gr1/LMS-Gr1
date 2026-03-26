using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;

namespace LMS.Infractructure.Repositories;
// https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Infractructure/Repositories/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;
	private readonly Lazy<ICourseRepository> courseRepository;
	private readonly Lazy<IModuleRepository> moduleRepository;
	public ICourseRepository CourseRepository => courseRepository.Value;
	public IModuleRepository ModuleRepository => moduleRepository.Value;
	public UnitOfWork(ApplicationDbContext context, Lazy<ICourseRepository> courseRepository, Lazy<IModuleRepository> moduleRepository)
    {
		this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(UnitOfWork.courseRepository));
		this.moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(UnitOfWork.moduleRepository));

		this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task CompleteAsync() => await context.SaveChangesAsync();

	
}
