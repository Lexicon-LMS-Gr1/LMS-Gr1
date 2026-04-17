using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Submission;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    public SubmissionService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<SubmissionDto> GetSubmissionByIdAsync(int submissionId, string currentUserId, bool isTeacher)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

        var submission = await _unitOfWork.SubmissionRepository.GetByIdAsync(submissionId);

        if (submission is null)
            throw new NotFoundException($"Inlämning med id {submissionId} hittades inte.");

        // Elever får endast se egna 
        if (submission.StudentId != currentUserId)
        {
            // Lärare får se alla submissions
            if (isTeacher == false)
                throw new ForbiddenException("Du saknar behörighet att se denna inlämning.");
        }

        return ToSubmissionDto(submission);
    }

    public async Task<IEnumerable<SubmissionDto>> GetSubmissionsForCourseAsync(int courseId, string currentUserId, bool isTeacher)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

        if (courseId <= 0)
            throw new BadRequestException("Ogiltigt kurs-id.", "Valideringsfel");

        var submissions = await _unitOfWork.SubmissionRepository.GetByCourseIdAsync(courseId);

        // Lärare får se alla, elever endast egna
        if (!isTeacher)
        {
            submissions = submissions.Where(s => s.StudentId == currentUserId);
        }

        return submissions.Select(s => ToSubmissionDto(s));
    }

    private static SubmissionDto ToSubmissionDto(Submission submission)
    {
        var dto = new SubmissionDto
        {
            Id = submission.Id,
            ActivityId = submission.ActivityId,
            StudentId = submission.StudentId,
            FileName = submission.FileName,
            Comment = submission.Comment,
            SubmittedAt = submission.SubmittedAt,
            IsLate = submission.IsLate,
            Feedback = submission.Feedback,
            FeedbackGivenAt = submission.FeedbackGivenAt,
            FeedbackGivenByTeacherId = submission.FeedbackGivenByTeacherId
        };

        string? teacherName = null;

        if (submission.FeedbackGivenByTeacher != null)
        {
            teacherName = $"{submission.FeedbackGivenByTeacher.FirstName} {submission.FeedbackGivenByTeacher.LastName}";
        }

        dto.FeedbackGivenByTeacherName = teacherName;

        return dto;
    }

    public async Task<SubmissionDto> SubmitAsync(int activityId, string studentId, string filePath, string fileName, string? comment)
    {
        if (activityId <= 0)
            throw new BadRequestException("Ogiltigt aktivitets-id.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(studentId))
            throw new BadRequestException("Elev-id saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(filePath))
            throw new BadRequestException("Filsökväg saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(fileName))
            throw new BadRequestException("Filnamn saknas.", "Valideringsfel");

        var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(activityId);
        if (activity == null)
            throw new NotFoundException("Aktiviteten hittades inte.");

        bool isLate = activity.DueDate.HasValue && DateTime.UtcNow > activity.DueDate.Value;

        var submission = new Submission
        {
            ActivityId = activityId,
            StudentId = studentId,
            FilePath = filePath,
            FileName = fileName,
            Comment = comment ?? "",
            SubmittedAt = DateTime.UtcNow,
            IsLate = isLate
        };

        _unitOfWork.SubmissionRepository.Add(submission);

        // Notification part
        var usersInCourse = await _userManager.Users.Where(u => u.CourseId == activity.Module.CourseId).ToListAsync();

        ApplicationUser? teacher = null;

        foreach (var user in usersInCourse)
        {
            if (await _userManager.IsInRoleAsync(user, "Teacher"))
            {
                teacher = user;
                break;
            }
        }

        if (teacher is not null)
        {
            var student = await _userManager.FindByIdAsync(studentId);

            var notification = new Notification
            {
                UserId = teacher.Id,
                Type = NotificationType.SubmissionCreated,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,

                ActorUserId = studentId,
                ActorName = student is not null
                    ? $"{student.FirstName} {student.LastName}"
                    : "En elev",

                CourseId = activity.Module.CourseId,
                CourseName = activity.Module.Course.Name,

                ActivityId = activity.Id,
                ActivityName = activity.Name,

                SubmissionId = submission.Id
            };

            _unitOfWork.NotificationRepository.Create(notification);
        }
        // end Notification part

        await _unitOfWork.CompleteAsync();

        return ToSubmissionDto(submission);
    }

    public async Task<(string FilePath, string FileName)> GetSubmissionFileInfoAsync(int submissionId)
    {
        var submission = await _unitOfWork.SubmissionRepository.GetByIdAsync(submissionId);
        if (submission == null)
            throw new NotFoundException($"Inlämning med id {submissionId} hittades inte.");

        return (submission.FilePath, submission.FileName);
    }

    public async Task GiveFeedbackAsync(int submissionId, string feedback, string teacherId)
    {
        if (string.IsNullOrWhiteSpace(teacherId))
            throw new BadRequestException("Lärar-id saknas.", "Valideringsfel");

        var submission = await _unitOfWork.SubmissionRepository.GetByIdAsync(submissionId);

        if (submission == null)
            throw new NotFoundException("Inlämning hittades inte.");

        submission.Feedback = feedback;
        submission.FeedbackGivenAt = DateTime.UtcNow;
        submission.FeedbackGivenByTeacherId = teacherId;

        var teacher = await _userManager.FindByIdAsync(teacherId);

        var notification = new Notification
        {
            UserId = submission.StudentId,
            Type = NotificationType.FeedbackReceived,
            CreatedAt = DateTime.UtcNow,
            IsRead = false,

            ActorUserId = teacherId,
            ActorName = teacher != null
                   ? $"{teacher.FirstName} {teacher.LastName}"
                   : null,

            ActivityId = submission.ActivityId,
            ActivityName = submission.Activity.Name,

            SubmissionId = submission.Id,

            Message = feedback
        };

        _unitOfWork.NotificationRepository.Create(notification);
        await _unitOfWork.CompleteAsync();
    }

    public async Task<IEnumerable<SubmissionListItemDto>> GetSubmissionsForActivityAsync(
        int activityId,
        string currentUserId,
        bool isTeacher)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
            throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

        if (activityId <= 0)
            throw new BadRequestException("Ogiltigt aktivitets-id.", "Valideringsfel");

        if (!isTeacher)
            throw new ForbiddenException("Du saknar behörighet att se inlämningar för denna aktivitet.");

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
            FileName = s.FileName,

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
}