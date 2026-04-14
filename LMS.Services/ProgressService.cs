using Domain.Contracts.Repositories;
using Domain.Models.Exceptions;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services;

public class ProgressService : IProgressService
{
	private readonly IUnitOfWork _unitOfWork;

	public ProgressService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}
	// Progressmetoderna ska kanske ska göras om till egen ProgressService
	public async Task<int> GetCourseProgressAsync(string userId, int courseId)
	{
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

        if (courseId <= 0)
            throw new BadRequestException("Ogiltigt kurs-id.", "Valideringsfel");

        // All activities for the course
        var activities = await _unitOfWork.ActivityRepository.GetByCourseIdAsync(courseId);

		var activityCount = activities.Count();
		if (activityCount == 0) return 0;

		// All submissions for the course
		var submissionActivities = await _unitOfWork.ActivityRepository.GetSubmissionActivitiesForCourseAsync(courseId);

		// All submissions by the user
		var submissions = await _unitOfWork.SubmissionRepository.GetByStudentIdAsync(userId);


		// All submission id's
		var submissionActivityIds = submissionActivities
			.Select(a => a.Id)
			.ToHashSet();

		// All submission id's by the user
		var submittedActivityIds = submissions
			.Select(s => s.ActivityId)
			.ToHashSet();

		// Current date
		var now = DateTime.UtcNow;

		var completedCount = activities.Count(activity => {
			var hasEnded = activity.EndTime <= now;
			var reqSubmission = submissionActivityIds.Contains(activity.Id);

			// If the activity require a submission it is completed only if a submission is made
			if (reqSubmission) {
				var hasSubmitted = submittedActivityIds.Contains(activity.Id);
				return hasSubmitted;
			}

			// No submission required, completed when it has ended
			return hasEnded;

		});

		// return whole percentage points
		return (int)Math.Round((double)completedCount / activityCount * 100);



	}

	public async Task<int> GetModuleProgressAsync(string userId, int moduleId)
	{
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

        if (moduleId <= 0)
            throw new BadRequestException("Ogiltigt modul-id.", "Valideringsfel");

        var activities = await _unitOfWork.ActivityRepository.GetByModuleIdAsync(moduleId);

		var activityCount = activities.Count();
		if (activityCount == 0) return 0;

		// All submissions for the activity
		var submissionActivities = await _unitOfWork.ActivityRepository.GetSubmissionActivitiesForModuleAsync(moduleId);

		// All submissions by the user
		var submissions = await _unitOfWork.SubmissionRepository.GetByStudentIdAsync(userId);

		// All submission id's
		var submissionActivityIds = submissionActivities
				.Select(a => a.Id)
				.ToHashSet();

		// All submission id's by the user
		var submittedActivityIds = submissions
			.Select(s => s.ActivityId)
			.ToHashSet();

		// Current date
		var now = DateTime.UtcNow;

		var completedCount = activities.Count(activity => {
			var hasEnded = activity.EndTime <= now;
			var reqSubmission = submissionActivityIds.Contains(activity.Id);

			// If the activity require a submission it is completed only if a submission is made
			if (reqSubmission) {
				var hasSubmitted = submittedActivityIds.Contains(activity.Id);
				return hasSubmitted;
			}

			// No submission required, completed when it has ended
			return hasEnded;

		});

		return (int)Math.Round((double)completedCount / activityCount * 100);

	}
}