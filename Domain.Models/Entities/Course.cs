using System.Reflection;

namespace Domain.Models.Entities;

public class Course
{
	public int Id { get; set; }

	public required string Name { get; set; }
	public required string Description { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

    // Navigation Property that represents the relationship between Course and ApplicationUser (Students)
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

	// Navigation Property that represents the relationship between Course and Module
	public ICollection<Module> Modules { get; set; } = new List<Module>();

	// Navigation Property that represents the relationship between Course and Document
	public ICollection<Document> Documents { get; set; } = new List<Document>();
}