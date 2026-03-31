using Domain.Contracts.Repositories;
using LMS.Infrastructure.Data;

namespace LMS.Infractructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext context;
    private readonly Lazy<ICourseRepository> courseRepository;
    private readonly Lazy<IModuleRepository> moduleRepository;
    private readonly Lazy<IActivityRepository> activityRepository;
    private readonly Lazy<IDocumentRepository> documentRepository;

    public ICourseRepository CourseRepository => courseRepository.Value;
    public IModuleRepository ModuleRepository => moduleRepository.Value;
    public IActivityRepository ActivityRepository => activityRepository.Value;
    public IDocumentRepository DocumentRepository => documentRepository.Value;

    public UnitOfWork(
        ApplicationDbContext context,
        Lazy<ICourseRepository> courseRepository,
        Lazy<IModuleRepository> moduleRepository,
        Lazy<IActivityRepository> activityRepository,
        Lazy<IDocumentRepository> documentRepository)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        this.moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
        this.activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
        this.documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
    }

    public async Task CompleteAsync() => await context.SaveChangesAsync();
}
