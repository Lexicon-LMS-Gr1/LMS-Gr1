using Domain.Contracts.Repositories;
using LMS.Shared.DTOs.Submission;
using LMS.Shared.DTOs.TeacherDashboard;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services;

public class SubmissionService : ISubmissionService
{
	private readonly IUnitOfWork _unitOfWork;

	public SubmissionService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<SubmissionDto?> GetSubmissionByIdAsync(int submissionId, string currentUserId, bool isTeacher)
	{
		var submission = await _unitOfWork.SubmissionRepository.GetByIdAsync(submissionId);

		if (submission is null)
			return null;

		// studenter får endast se egna 
		if (submission.StudentId != currentUserId) {
			// lärare får se alla submissions
			if (isTeacher == false)
				return null;
		}



		return ToSubmissionDto(submission);
	}

	public async Task<IEnumerable<SubmissionDto>> GetSubmissionsForCourseAsync(int courseId, string currentUserId, bool isTeacher)
	{
		var submissions = await _unitOfWork.SubmissionRepository.GetByCourseIdAsync(courseId);

		// Lärare får se alla, studenter bara egna
		if (!isTeacher) {
			submissions = submissions.Where(s => s.StudentId == currentUserId);
		}

		return submissions.Select(s => ToSubmissionDto(s));
	}


	private static SubmissionDto ToSubmissionDto(Submission submission)
	{
		var dto = new SubmissionDto {
			Id = submission.Id,
			ActivityId = submission.ActivityId,
			StudentId = submission.StudentId,
			FileName = submission.FileName,
			Comment = submission.Comment,
			SubmittedAt = submission.SubmittedAt,
			Feedback = submission.Feedback,
			FeedbackGivenAt = submission.FeedbackGivenAt,
			FeedbackGivenByTeacherId = submission.FeedbackGivenByTeacherId
		};

		string? teacherName = null;

		if (submission.FeedbackGivenByTeacher != null) {
			teacherName = $"{submission.FeedbackGivenByTeacher.FirstName} {submission.FeedbackGivenByTeacher.LastName}";
		}

		dto.FeedbackGivenByTeacherName = teacherName;

		return dto;
	}

    public async Task<IEnumerable<SubmissionListItemDto>> GetSubmissionsForActivityAsync(
    int activityId,
    string currentUserId,
    bool isTeacher)
    {
        if (!isTeacher)
            return Enumerable.Empty<SubmissionListItemDto>();

        var submissions = await _unitOfWork.SubmissionRepository.GetByActivityIdAsync(activityId);

        return submissions.Select(s => new SubmissionListItemDto
        {
            SubmissionId = s.Id,
            StudentId = s.StudentId,
            StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
            StudentEmail = s.Student.Email,

            CourseName = s.Activity.Module.Course.Name,
            ModuleName = s.Activity.Module.Name,
            ActivityName = s.Activity.Name,

            SubmittedAt = s.SubmittedAt,

            HasFeedback = !string.IsNullOrWhiteSpace(s.Feedback),
            FeedbackGivenAt = s.FeedbackGivenAt
        });
    }

    public async Task<IEnumerable<SubmissionListItemDto>> GetAllSubmissionsAsync()
    {
        var submissions = await _unitOfWork.SubmissionRepository.GetAllAsync();

        return submissions.Select(s => new SubmissionListItemDto
        {
            SubmissionId = s.Id,
            StudentId = s.StudentId,
            StudentName = $"{s.Student.FirstName} {s.Student.LastName}",
            StudentEmail = s.Student.Email,

            CourseName = s.Activity.Module.Course.Name,
            ModuleName = s.Activity.Module.Name,
            ActivityName = s.Activity.Name,
            ActivityId = s.ActivityId,

            SubmittedAt = s.SubmittedAt,
            HasFeedback = !string.IsNullOrWhiteSpace(s.Feedback),
            FeedbackGivenAt = s.FeedbackGivenAt
        });
    }

    public async Task GiveFeedbackAsync(int submissionId, string feedback, string teacherId)
    {
        var submission = await _unitOfWork.SubmissionRepository.GetByIdAsync(submissionId);

        if (submission == null)
            throw new Exception("Submission not found");

        submission.Feedback = feedback;
        submission.FeedbackGivenAt = DateTime.UtcNow;
        submission.FeedbackGivenByTeacherId = teacherId;

        await _unitOfWork.CompleteAsync();
    }


}
