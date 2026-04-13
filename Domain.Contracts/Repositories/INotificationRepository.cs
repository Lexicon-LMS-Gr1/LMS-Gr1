using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface INotificationRepository : IRepositoryBase<Notification>
{
	Task<Notification?> GetByIdAsync(int id, bool trackChanges = false);
	Task<IEnumerable<Notification>> GetByUserIdAsync(string userId, bool trackChanges = false);
}