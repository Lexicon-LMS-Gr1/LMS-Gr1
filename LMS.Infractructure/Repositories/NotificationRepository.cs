using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infractructure.Repositories;

public class NotificationRepository(ApplicationDbContext context) : RepositoryBase<Notification>(context), INotificationRepository
{
	public async Task<Notification?> GetByIdAsync(int id, bool trackChanges = false)
	{
		return await FindByCondition(n => n.Id == id, trackChanges).FirstOrDefaultAsync();
	}

	public async Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool trackChanges = false)
	{
		return await FindByCondition(n => n.UserId == userId, trackChanges).OrderByDescending(n => n.CreatedAt).ToListAsync();
	}
}