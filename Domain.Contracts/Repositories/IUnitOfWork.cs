namespace Domain.Contracts.Repositories;

public interface IUnitOfWork
{
	ICourseRepository CourseRepository { get; }
	IModuleRepository ModuleRepository { get; }
    IActivityRepository ActivityRepository { get; }
    IDocumentRepository DocumentRepository { get; }
	Task CompleteAsync();
}