using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Domain.Models.Entities;

public class Module
{
	public int Id { get; set; }

	public required string Name { get; set; }
	public required string Description { get; set; }

	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }

	// Foreign Key to Course
	public int CourseId { get; set; }

	// Navigation Property the relationship between Module and Course
	public required Course Course { get; set; }

	// Navigation Property that represents the relationship between Module and Activity
	public ICollection<Activity> Activities { get; set; } = new List<Activity>();
}
}