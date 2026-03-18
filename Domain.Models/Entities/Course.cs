using System.Reflection;

namespace Domain.Models.Entities;

public class Course
{
	public int Id { get; set; }

	public required string Name { get; set; }
	public required string Description { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	// Foreign Key to ApplicationUser (Teacher)
	public string TeacherId { get; set; } = default!;

    // Navigation Property that represents the relationship between Course and ApplicationUser (Teacher)
    public ApplicationUser Teacher { get; set; } = default!;

    // Navigation Property that represents the relationship between Course and ApplicationUser (Students)
    public ICollection<ApplicationUser> Students { get; set; } = new List<ApplicationUser>();

	// Navigation Property that represents the relationship between Course and Module
	public ICollection<Module> Modules { get; set; } = new List<Module>();
}