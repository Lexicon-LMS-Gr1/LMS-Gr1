namespace Domain.Models.Entities;

public class ActivityType
{
	public int Id { get; set; }

	public required string Name { get; set; }

	// Navigation Property that represents the relationship between ActivityType and Activity
	public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}