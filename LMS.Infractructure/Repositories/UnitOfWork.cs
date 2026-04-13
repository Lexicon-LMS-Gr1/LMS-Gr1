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
    private readonly Lazy<ISubmissionRepository> submissionRepository;
    private readonly Lazy<INotificationRepository> notificationRepository;

	public ICourseRepository CourseRepository => courseRepository.Value;
    public IModuleRepository ModuleRepository => moduleRepository.Value;
    public IActivityRepository ActivityRepository => activityRepository.Value;
    public IDocumentRepository DocumentRepository => documentRepository.Value;
    public ISubmissionRepository SubmissionRepository => submissionRepository.Value;
    public INotificationRepository NotificationRepository => notificationRepository.Value;

	public UnitOfWork(
        ApplicationDbContext context,
        Lazy<ICourseRepository> courseRepository,
        Lazy<IModuleRepository> moduleRepository,
        Lazy<IActivityRepository> activityRepository,
        Lazy<IDocumentRepository> documentRepository,
        Lazy<ISubmissionRepository> submissionRepository,
        Lazy<INotificationRepository> notificationRepository
		)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        this.moduleRepository = moduleRepository ?? throw new ArgumentNullException(nameof(moduleRepository));
        this.activityRepository = activityRepository ?? throw new ArgumentNullException(nameof(activityRepository));
        this.documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
        this.submissionRepository = submissionRepository ?? throw new ArgumentNullException(nameof(submissionRepository));
        this.notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
	}

    public async Task CompleteAsync() => await context.SaveChangesAsync();
}
