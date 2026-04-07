namespace Service.Contracts;

public interface IProgressService
{
	Task<int> GetCourseProgressAsync(string userId, int courseId);

	Task<int> GetModuleProgressAsync(string userId, int moduleId);
}