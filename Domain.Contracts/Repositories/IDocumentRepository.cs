using Domain.Models.Entities;

namespace Domain.Contracts.Repositories;

public interface IDocumentRepository : IRepositoryBase<Document>
{
    Task<Document?> GetByIdAsync(int id, bool trackChanges = false);
    Task<IEnumerable<Document>> GetByCourseIdAsync(int courseId, bool trackChanges = false);
    Task<IEnumerable<Document>> GetByModuleIdAsync(int moduleId, bool trackChanges = false);
    Task<IEnumerable<Document>> GetByActivityIdAsync(int activityId, bool trackChanges = false);
}
