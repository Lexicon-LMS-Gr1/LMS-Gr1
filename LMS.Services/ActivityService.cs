using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Services.Validation;
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

    public async Task<ActivityDto> GetActivityByIdAsync(int id)
    {
        var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(id);

        if (activity is null)
            throw new NotFoundException($"Aktivitet med id {id} hittades inte.");

        return MapToDto(activity);
    }

    public async Task<ActivityDto> CreateActivityAsync(int moduleId, ActivityCreateDto activityDto)
    {
        if (activityDto is null)
            throw new BadRequestException("Aktivitetsdata saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(activityDto.Name))
            throw new BadRequestException("Aktivitetsnamn saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(activityDto.Description))
            throw new BadRequestException("Aktivitetsbeskrivning saknas.", "Valideringsfel");

        // Affärssregel: aktivitet startar alltid 08:00 och slutar alltid 17:00
        var normalizedStart = activityDto.StartTime.Date.AddHours(8);
        var normalizedEnd = activityDto.EndTime.Date.AddHours(17);

        if (normalizedStart > normalizedEnd)
            throw new BadRequestException("Aktivitetens starttidpunkt kan inte ligga efter sluttidpunkt.", "Valideringsfel");

        var module = await _unitOfWork.ModuleRepository.GetModuleWithActivitiesAsync(moduleId, trackChanges: false);

        if (module is null)
            throw new NotFoundException($"Modul med id \"{moduleId}\" hittades inte.");

        // Aktiviteten måste ligga inom modulens datumintervall
        if (normalizedStart.Date < module.StartDate.Date || normalizedEnd.Date > module.EndDate.Date)
            throw new BadRequestException("Aktiviteten ligger utanför modulens datumintervall.", "Valideringsfel");

        if (!await _unitOfWork.ActivityRepository.ActivityTypeExistsAsync(activityDto.ActivityTypeId))
            throw new BadRequestException("Angiven aktivitetstyp finns inte.", "Valideringsfel");

        DateTime? normalizedDueDate = null;

        // Om man valt ett duedate (deadline) så måste det ligga inom aktivitetens datumintervall, sätts till kl 17.00 den dagen
        if (activityDto.DueDate.HasValue)
        {
            normalizedDueDate = activityDto.DueDate.Value.Date.AddHours(17);

            if (normalizedDueDate < normalizedStart || normalizedDueDate > normalizedEnd)
                throw new BadRequestException("Deadline måste ligga inom aktivitetens datumintervall.", "Valideringsfel");
        }

        // Regeln är "en aktivitet per dag och modul" och en aktivitet kan sträcka sig över flera dagar,
        // Då räcker det med en vanlig intervallöverlapp för att stoppa alla krockar.
        bool overlaps = module.Activities.Any(a =>
            normalizedStart < a.EndTime && normalizedEnd > a.StartTime);

        if (overlaps)
            throw new BadRequestException("Aktiviteten överlappar en annan aktivitet i modulen.", "Valideringsfel");

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
            DueDate = normalizedDueDate,
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

   	public async Task<ActivityDto> UpdateActivityAsync(ActivityUpdateDto dto)
	{
		if (dto is null)
            throw new BadRequestException("Aktivitetsdata saknas.", "Valideringsfel");

        if (string.IsNullOrWhiteSpace(dto.Name))
			throw new BadRequestException("Namn saknas.", "Valideringsfel");

		if (string.IsNullOrWhiteSpace(dto.Description))
			throw new BadRequestException("Beskrivning saknas.", "Valideringsfel");

		var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(dto.Id, trackChanges: true);

		if (activity is null)
			throw new NotFoundException("Aktiviteten saknas.");

		var module = await _unitOfWork.ModuleRepository.GetModuleWithActivitiesAsync(activity.ModuleId, trackChanges: false);

		if (module is null)
			throw new NotFoundException("Modul saknas.");

		var normalizedStart = dto.StartTime.Date.AddHours(8);
		var normalizedEnd = dto.EndTime.Date.AddHours(17);
		var normalizedDueDate = dto.DueDate?.Date.AddHours(17);

		ActivityDateValidator.Validate(normalizedStart, normalizedEnd, normalizedDueDate, module, activity.Id);

		activity.Name = dto.Name.Trim();
		activity.Description = dto.Description.Trim();
		activity.StartTime = normalizedStart;
		activity.EndTime = normalizedEnd;
		activity.DueDate = normalizedDueDate;

		await _unitOfWork.CompleteAsync();

		return MapToDto(activity);
	}

    public async Task DeleteActivityAsync(int id)
    {
        var activity = await _unitOfWork.ActivityRepository.GetByIdAsync(id, trackChanges: true);

        if (activity is null)
            throw new NotFoundException($"Aktivitet med id {id} hittades inte.");

        _unitOfWork.ActivityRepository.Delete(activity);
        await _unitOfWork.CompleteAsync();
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