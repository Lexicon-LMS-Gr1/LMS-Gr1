namespace Domain.Models.Entities;

public class Activity
{
	public int Id { get; set; }

	public required string Name { get; set; }
	public required string Description { get; set; }

	public DateTime StartTime { get; set; }
	public DateTime EndTime { get; set; }
	public DateTime? DueDate { get; set; }

	// Foreign Key to ActivityType
	public int ActivityTypeId { get; set; }

	// Foreign Key to Module
	public int ModuleId { get; set; }

	// Navigation Property that represents the relationship between Activity and ActivityType
	public required ActivityType ActivityType { get; set; }

	// Navigation Property that represents the relationship between Activity and Module
	public required Module Module { get; set; }

	// Navigation Property that represents the relationship between Activity and Document
	public ICollection<Document> Documents { get; set; } = new List<Document>();
}