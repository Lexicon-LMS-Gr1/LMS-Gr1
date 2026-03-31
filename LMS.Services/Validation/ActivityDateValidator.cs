using Domain.Models.Entities;
using LMS.Shared.DTOs.Activity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services.Validation;

public static class ActivityDateValidator
{
	public static void Validate(
		UpdateActivityDto dto,
		Module module,
		int currentActivityId)
	{
		var startDate = dto.StartTime.Date;
		var endDate = dto.EndTime.Date;
		var moduleStart = module.StartDate.Date;
		var moduleEnd = module.EndDate.Date;

		// tillåter samma dag
		if (startDate > endDate)
			throw new ArgumentException("Startdatum måste vara samma som eller före slutdatum.");

		if (startDate < moduleStart || endDate > moduleEnd)
			throw new ArgumentException("Aktivitetens datum måste ligga inom modulens datumintervall.");

		if (dto.DueDate.HasValue && dto.DueDate.Value.Date < startDate)
			throw new ArgumentException("Deadline kan inte vara före aktivitetens startdatum.");

		bool overlaps = module.Activities
			.Where(a => a.Id != currentActivityId)
			.Any(a => startDate <= a.EndTime.Date && endDate >= a.StartTime.Date);

		if (overlaps)
			throw new ArgumentException("Aktiviteten överlappar en annan aktivitet i modulen.");
	}
}