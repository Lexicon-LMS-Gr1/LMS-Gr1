using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;

namespace LMS.Infractructure.Repositories;
// https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Infractructure/Repositories/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;
	private readonly Lazy<ICourseRepository> courseRepository;
	private readonly Lazy<IModuleRepository> moduleRepository;
    private readonly Lazy<IActivityRepository> activityRepository;
    private readonly Lazy<ISubmissionRepository> submissionRepository;
    public ICourseRepository CourseRepository => courseRepository.Value;
	public IModuleRepository ModuleRepository => moduleRepository.Value;
    public IActivityRepository ActivityRepository => activityRepository.Value;
    public ISubmissionRepository SubmissionRepository => submissionRepository.Value;
    public UnitOfWork(ApplicationDbContext context, Lazy<ICourseRepository> courseRepository, 
        Lazy<IModuleRepository> moduleRepository, Lazy<IActivityRepository> activityRepository, Lazy<ISubmissionRepository> submissionRepository)
    {
		this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(UnitOfWork.courseRepository));
		this.moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(UnitOfWork.moduleRepository));
        this.activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(UnitOfWork.activityRepository));
        this.submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(UnitOfWork.submissionRepository));

        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task CompleteAsync() => await context.SaveChangesAsync();

	
}
