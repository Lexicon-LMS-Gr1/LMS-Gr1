namespace Domain.Contracts.Repositories;

public interface IUnitOfWork
{
	ICourseRepository CourseRepository { get; }
	IModuleRepository ModuleRepository { get; }
    IActivityRepository ActivityRepository { get; }
	Task CompleteAsync();
}