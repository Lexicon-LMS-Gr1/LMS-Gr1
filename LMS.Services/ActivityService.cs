using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Shared.DTOs.Activity;
using Service.Contracts;

namespace LMS.Services;

public class ActivityService : IActivityService
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync()
    {
        var activities = await _unitOfWork.ActivityRepository.GetAllAsync();

        return activities.Select(MapToDto);
    }

    public async Task<IEnumerable<ActivityDto>> GetActivitiesByModuleIdAsync(int moduleId)
    {
        var activities = await _unitOfWork.ActivityRepository.GetByModuleIdAsync(moduleId);

        return activities.Select(MapToDto);
    }

    public async Task<ActivityDto?> GetActivityByIdAsync(int id)
    {
        var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(id);

        return activity is null ? null : MapToDto(activity);
    }

    public async Task<ActivityDto> CreateActivityAsync(int moduleId, ActivityCreateDto activityDto)
    {
        if (activityDto is null)
            throw new ArgumentNullException(nameof(activityDto));

        if (string.IsNullOrWhiteSpace(activityDto.Name))
            throw new ArgumentException("Aktivitetsnamn saknas.");

        if (string.IsNullOrWhiteSpace(activityDto.Description))
            throw new ArgumentException("Aktivitetsbeskrivning saknas.");

        // Businessregel: aktivitet startar alltid 08:00 och slutar alltid 17:00
        var normalizedStart = activityDto.StartTime.Date.AddHours(8);
        var normalizedEnd = activityDto.EndTime.Date.AddHours(17);

        if (normalizedStart > normalizedEnd)
            throw new ArgumentException("Aktivitetens starttidpunkt kan inte ligga efter sluttidpunkt.");

        var module = await _unitOfWork.ModuleRepository.GetModuleWithActivitiesAsync(moduleId, trackChanges: false);

        if (module is null)
            throw new KeyNotFoundException($"Modul med id \"{moduleId}\" hittades inte.");

        // Modulens StartDate/EndDate används som datumgränser
        if (normalizedStart.Date < module.StartDate.Date || normalizedEnd.Date > module.EndDate.Date)
            throw new ArgumentException("Aktiviteten ligger utanför modulens datumintervall.");

        if (!await _unitOfWork.ActivityRepository.ActivityTypeExistsAsync(activityDto.ActivityTypeId))
            throw new ArgumentException("Angiven aktivitetstyp finns inte.");

        if (activityDto.DueDate.HasValue && activityDto.DueDate.Value > normalizedEnd)
            throw new ArgumentException("Förfallodatum kan inte ligga efter aktivitetens sluttidpunkt.");

        // Regeln är "en aktivitet per dag och modul" och en aktivitet kan sträcka sig över flera dagar,
        // Då räcker det med en vanlig intervallöverlapp för att stoppa alla krockar.
        bool overlaps = module.Activities.Any(a =>
            normalizedStart < a.EndTime && normalizedEnd > a.StartTime);

        if (overlaps)
            throw new ArgumentException("Aktiviteten överlappar en annan aktivitet i modulen.");

        // Sätt endast FK-värden vid create.
        // Navigation properties behöver inte sättas här och kan orsaka problem
        // om relaterade entiteter hämtats med AsNoTracking().
        // EF sätter relationer via ActivityTypeId och ModuleId men navigationen kan laddas senare
        var activity = new Activity
        {
            Name = activityDto.Name.Trim(),
            Description = activityDto.Description.Trim(),
            StartTime = normalizedStart,
            EndTime = normalizedEnd,
            DueDate = activityDto.DueDate,
            ActivityTypeId = activityDto.ActivityTypeId,
            ModuleId = moduleId,
            Module = null!,
            ActivityType = null!
        };

        _unitOfWork.ActivityRepository.Create(activity);
        await _unitOfWork.CompleteAsync();

        var savedActivity = await _unitOfWork.ActivityRepository.GetByIdAsync(activity.Id);

        if (savedActivity is null)
            throw new InvalidOperationException("Aktiviteten skapades, men kunde inte läsas tillbaka.");

        return MapToDto(savedActivity);
    }

    public async Task<ActivityDto> UpdateActivityAsync(ActivityUpdateDto activityDto)
    {
        if (activityDto is null)
            throw new ArgumentNullException(nameof(activityDto));

        var existingActivity = await _unitOfWork.ActivityRepository.GetByIdAsync(activityDto.Id, trackChanges: true);

        if (existingActivity is null)
            throw new KeyNotFoundException($"Aktivitet med id \"{activityDto.Id}\" hittades inte.");

        if (string.IsNullOrWhiteSpace(activityDto.Name))
            throw new ArgumentException("Aktivitetsnamn saknas.");

        if (string.IsNullOrWhiteSpace(activityDto.Description))
            throw new ArgumentException("Aktivitetsbeskrivning saknas.");

        var normalizedStart = activityDto.StartTime.Date.AddHours(8);
        var normalizedEnd = activityDto.EndTime.Date.AddHours(17);

        if (normalizedStart > normalizedEnd)
            throw new ArgumentException("Aktivitetens starttidpunkt kan inte ligga efter sluttidpunkt.");

        var module = await _unitOfWork.ModuleRepository.GetModuleWithActivitiesAsync(existingActivity.ModuleId, trackChanges: false);

        if (module is null)
            throw new KeyNotFoundException($"Modul med id \"{existingActivity.ModuleId}\" hittades inte.");

        if (normalizedStart.Date < module.StartDate.Date || normalizedEnd.Date > module.EndDate.Date)
            throw new ArgumentException("Aktiviteten ligger utanför modulens datumintervall.");

        if (!await _unitOfWork.ActivityRepository.ActivityTypeExistsAsync(activityDto.ActivityTypeId))
            throw new ArgumentException("Angiven aktivitetstyp finns inte.");

        if (activityDto.DueDate.HasValue && activityDto.DueDate.Value > normalizedEnd)
            throw new ArgumentException("Förfallodatum kan inte ligga efter aktivitetens sluttidpunkt.");

        bool overlaps = module.Activities
            .Where(a => a.Id != activityDto.Id)
            .Any(a => normalizedStart < a.EndTime && normalizedEnd > a.StartTime);

        if (overlaps)
            throw new ArgumentException("Aktiviteten överlappar en annan aktivitet i modulen.");

        existingActivity.Name = activityDto.Name.Trim();
        existingActivity.Description = activityDto.Description.Trim();
        existingActivity.StartTime = normalizedStart;
        existingActivity.EndTime = normalizedEnd;
        existingActivity.DueDate = activityDto.DueDate;
        existingActivity.ActivityTypeId = activityDto.ActivityTypeId;

        _unitOfWork.ActivityRepository.Update(existingActivity);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.ActivityRepository.GetByIdAsync(existingActivity.Id);

        if (updated is null)
            throw new InvalidOperationException("Aktiviteten uppdaterades, men kunde inte läsas tillbaka.");

        return MapToDto(updated);
    }

    public async Task<bool> DeleteActivityAsync(int id)
    {
        var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(id, trackChanges: true);

        if (activity is null)
            return false;

        _unitOfWork.ActivityRepository.Delete(activity);
        await _unitOfWork.CompleteAsync();

        return true;
    }

    public async Task<IEnumerable<ActivityTypeDto>> GetAllActivityTypesAsync()
    {
        var activityTypes = await _unitOfWork.ActivityRepository.GetAllActivityTypesAsync();

        return activityTypes.Select(at => new ActivityTypeDto
        {
            Id = at.Id,
            Name = at.Name
        });
    }

    private static ActivityDto MapToDto(Activity activity)
    {
        return new ActivityDto
        {
            Id = activity.Id,
            Name = activity.Name,
            Description = activity.Description,
            StartTime = activity.StartTime,
            EndTime = activity.EndTime,
            DueDate = activity.DueDate,
            ActivityTypeId = activity.ActivityTypeId,
            ActivityTypeName = activity.ActivityType?.Name ?? string.Empty
        };
    }
}