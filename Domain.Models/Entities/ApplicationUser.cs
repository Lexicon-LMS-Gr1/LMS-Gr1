using Microsoft.AspNetCore.Identity;

namespace Domain.Models.Entities;

public class ApplicationUser : IdentityUser
{
	public required string FirstName { get; set; }
	public required string LastName { get; set; }

	public string? RefreshToken { get; set; }
	public DateTime RefreshTokenExpireTime { get; set; }

	public int? CourseId { get; set; }

	// Navigation Property that represents the relationship between ApplicationUser and Course
	public Course? Course { get; set; }
}