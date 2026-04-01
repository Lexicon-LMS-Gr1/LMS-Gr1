using Domain.Models.Entities;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;

namespace LMS.Services.Mappers;

public static class CourseMapper
{
	public static CourseListDto ToCourseListDto(Course course)
	{
		return new CourseListDto {
			Id = course.Id,
			Name = course.Name,
			Description = course.Description,
			StartDate = course.StartDate,
			EndDate = course.EndDate,
			StudentCount = course.Students.Count,
			ModuleCount = course.Modules.Count
		};
	}

	public static CourseDto ToBasicCourseDto(Course course)
	{
		return new CourseDto {
			Id = course.Id,
			Name = course.Name,
			Description = course.Description,
			StartDate = course.StartDate,
			EndDate = course.EndDate,
		};
	}

	public static CourseDto ToDetailedCourseDto(Course course)
	{
		return new CourseDto {
			Id = course.Id,
			Name = course.Name,
			Description = course.Description,
			StartDate = course.StartDate,
			EndDate = course.EndDate,
			Modules = course.Modules.Select(m => new ModuleDto {
				Id = m.Id,
				Name = m.Name,
				Description = m.Description,
				StartDate = m.StartDate,
				EndDate = m.EndDate,
				Activities = m.Activities.Select(a => new ActivityDto {
					Id = a.Id,
					Name = a.Name,
					Description = a.Description,
					StartTime = a.StartTime,
					EndTime = a.EndTime,
					DueDate = a.DueDate,
					ActivityTypeName = a.ActivityType.Name
				}).ToList()
			}).ToList()
		};
	}
}