using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.Activity;

public class UpdateActivityDto
{
	public int Id { get; set; }
	[Required(ErrorMessage = "Namn saknas.")]
	[StringLength(100, ErrorMessage = "Namnet får vara max 100 tecken.")]
	public string Name { get; set; } = string.Empty;

	[Required(ErrorMessage = "Beskrivning saknas")]
	[StringLength(1000, ErrorMessage = "Beskrivningen får vara max 1000 tecken.")]
	public string Description { get; set; } = string.Empty;

	[Required(ErrorMessage = "Starttid saknas.")]
	public DateTime StartTime { get; set; }

	[Required(ErrorMessage = "Sluttid saknas.")]
	public DateTime EndTime { get; set; }
	public DateTime? DueDate { get; set; }
}

