using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Notification;
using Service.Contracts;

namespace LMS.Services;

public class NotificationService : INotificationService
{
	private readonly IUnitOfWork _unitOfWork;

	public NotificationService(IUnitOfWork unitOfWork)
	{
		_unitOfWork = unitOfWork;
	}

	public async Task<IEnumerable<NotificationDto>> GetUnreadByUserIdAsync(string userId)
	{
		var notifications = await _unitOfWork.NotificationRepository
			.GetByUserIdAsync(userId, trackChanges: false);

		return notifications
			.Where(n => !n.IsRead)
			.OrderByDescending(n => n.CreatedAt)
			.Select(MapToDto);
	}

	public async Task MarkAsReadAsync(int notificationId, string userId)
	{
        if (string.IsNullOrWhiteSpace(userId))
            throw new BadRequestException("Användar-id saknas.", "Valideringsfel");

        var notification = await _unitOfWork.NotificationRepository
			.GetByIdAsync(notificationId, trackChanges: true);

		if (notification is null)
			throw new NotFoundException($"Notis med id {notificationId} hittades inte.");

		if (notification.UserId != userId)
			throw new ForbiddenException("Du saknar behörighet att markera denna notis som läst.");

		if (notification.IsRead)
			return;

		notification.IsRead = true;

		await _unitOfWork.CompleteAsync();
	}

	private static NotificationDto MapToDto(Notification n)
	{
		return new NotificationDto
		{
			Id = n.Id,
			Type = n.Type,
			CreatedAt = n.CreatedAt,
			IsRead = n.IsRead,
			ActorName = n.ActorName,
			CourseName = n.CourseName,
			ModuleName = n.ModuleName,
			ActivityName = n.ActivityName,
			DocumentName = n.DocumentName,
			SubmissionId = n.SubmissionId,
			Message = n.Message
		};
	}
}
