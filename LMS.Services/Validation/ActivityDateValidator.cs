using Domain.Models.Entities;

namespace LMS.Services.Validation;

public static class ActivityDateValidator
{
	public static void Validate(
		DateTime startTime,
		DateTime endTime,
		DateTime? dueDate,
		Module module,
		int currentActivityId)
	{
		var moduleStart = module.StartDate.Date;
		// Gör om tiden och gör den till hela dagen så hela dagen fram till  23:59:59.9999999
		var moduleEnd = module.EndDate.Date.AddDays(1).AddTicks(-1);

		if (startTime > endTime)
			throw new ArgumentException("Starttid måste vara samma som eller före sluttid.");

		if (startTime < moduleStart || endTime > moduleEnd)
			throw new ArgumentException("Aktivitetens tid måste ligga inom modulens datumintervall.");

		if (dueDate.HasValue && dueDate.Value < startTime)
			throw new ArgumentException("Deadline kan inte vara före aktivitetens starttid.");

		bool overlaps = module.Activities
			.Where(a => a.Id != currentActivityId)
			.Any(a => startTime < a.EndTime && endTime > a.StartTime);

		if (overlaps)
			throw new ArgumentException("Aktiviteten överlappar en annan aktivitet i modulen.");
	}
}