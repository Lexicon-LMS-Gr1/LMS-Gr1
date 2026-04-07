using LMS.Shared.DTOs.Submission;
namespace Service.Contracts;

public interface ISubmissionService
{
	Task<SubmissionDto?> GetSubmissionByIdAsync(int submissionId, string currentUserId, bool isTeacher);
	Task<IEnumerable<SubmissionDto>> GetSubmissionsForCourseAsync(int courseId, string currentUserId, bool isTeacher);
}
