using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infractructure.Repositories;

public class DocumentRepository : RepositoryBase<Document>, IDocumentRepository
{
    public DocumentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Document?> GetByIdAsync(int id, bool trackChanges = false)
    {
        return await FindByCondition(d => d.Id == id, trackChanges)
            .Include(d => d.UploadedBy)
            .Include(d => d.Course)
            .Include(d => d.Module)
            .Include(d => d.Activity)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Document>> GetByCourseIdAsync(int courseId, bool trackChanges = false)
    {
        return await FindByCondition(d => d.CourseId == courseId, trackChanges)
            .Include(d => d.UploadedBy)
            .OrderByDescending(d => d.UploadTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByModuleIdAsync(int moduleId, bool trackChanges = false)
    {
        return await FindByCondition(d => d.ModuleId == moduleId, trackChanges)
            .Include(d => d.UploadedBy)
            .OrderByDescending(d => d.UploadTimestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<Document>> GetByActivityIdAsync(int activityId, bool trackChanges = false)
    {
        return await FindByCondition(d => d.ActivityId == activityId, trackChanges)
            .Include(d => d.UploadedBy)
            .OrderByDescending(d => d.UploadTimestamp)
            .ToListAsync();
    }
}
