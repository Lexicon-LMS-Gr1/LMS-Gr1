using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
	private readonly IServiceManager _serviceManager;

	public NotificationsController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
	}

	[HttpGet("unread")]
	public async Task<IActionResult> GetUnreadNotifications()
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
		if (string.IsNullOrEmpty(userId))
			return Unauthorized();
		var notifications = await _serviceManager.NotificationService.GetUnreadByUserIdAsync(userId);
		return Ok(notifications);
	}

	[HttpPatch("{id}/read")]
	public async Task<IActionResult> MarkAsRead(int id)
	{
		var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
		if (string.IsNullOrEmpty(userId))
			return Unauthorized();
		await _serviceManager.NotificationService.MarkAsReadAsync(id, userId);
		return NoContent();
	}
}