using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Services.Mappers;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace LMS.Services
{
	// https://github.com/Lexicon-NET-2025-HT/CompaniesAPI/blob/master/Companies.Services/EmployeeService.cs
	public class CourseService : ICourseService
	{
		private readonly IUnitOfWork _unitOfWork;

		public CourseService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<CourseListDto>> GetAllCoursesListAsync()
		{
			var courses = await _unitOfWork.CourseRepository.GetCoursesForListAsync();

			return courses.Select(CourseMapper.ToCourseListDto);
		}

		public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
		{
			var courses = await _unitOfWork.CourseRepository.GetAllAsync();

			return courses.Select(CourseMapper.ToBasicCourseDto);
		}

		public async Task<CourseDto?> GetCourseForUserAsync(string userId)
		{
			var course = await _unitOfWork.CourseRepository.GetCourseForUserAsync(userId);

			if (course == null)
				return null;

			var dto = CourseMapper.ToDetailedCourseDto(course);
			dto.Progress = await GetCourseProgressAsync(userId, course.Id);

			return dto;
		}

		public async Task<CourseDto> CreateCourseAsync(CourseCreateDto courseCreateDto)
		{
			// DTOn får ej vara null + Grundläggande validering av kursens egna fält
			if (courseCreateDto is null)
				throw new ArgumentNullException(nameof(courseCreateDto));

			if (string.IsNullOrWhiteSpace(courseCreateDto.Name))
				throw new ArgumentException("Kursnamn saknas.");

			if (string.IsNullOrWhiteSpace(courseCreateDto.Description))
				throw new ArgumentException("Kursbeskrivning saknas.");

			// Kursens startdatum måste vara <= slutdatum
			if (courseCreateDto.StartDate > courseCreateDto.EndDate)
				throw new ArgumentException("Startdatum kan inte ligga efter slutdatum.");

			// Modules är en lista (initierad till tom lista i DTOn)
			var modules = courseCreateDto.Modules;

			// Validera varje modul individuellt
			foreach (var module in modules) {
				// TODO: Delvis duplicering av DataAnnotations-validerigen. Ev centralisera denna validering senare.

				if (string.IsNullOrWhiteSpace(module.Name))
					throw new ArgumentException("En modul saknar namn.");

				if (string.IsNullOrWhiteSpace(module.Description))
					throw new ArgumentException($"Modul '{module.Name}' saknar beskrivning.");

				// Modulens startdatum <= slutdatum
				if (module.StartDate > module.EndDate)
					throw new ArgumentException($"Modul '{module.Name}' har ett startdatum som ligger efter slutdatum.");

				// Modul måste ligga inom kursens datumintervall
				if (module.StartDate < courseCreateDto.StartDate || module.EndDate > courseCreateDto.EndDate)
					throw new ArgumentException($"Modul '{module.Name}' har datum som ligger utanför kursens datum.");
			}

			// Kontrollera att moduler inte överlappar varandra (inom samma request)
			for (int i = 0; i < modules.Count; i++) {
				for (int j = i + 1; j < modules.Count; j++) {
					var a = modules[i];
					var b = modules[j];

					// Intervallöverlapp
					bool overlaps = a.StartDate <= b.EndDate && a.EndDate >= b.StartDate;

					if (overlaps) {
						throw new ArgumentException(
							$"Modulerna '{a.Name}' och '{b.Name}' överlappar varandra.");
					}
				}
			}

			// TODO: Om man senare tillåter att lägga till moduler i en befintlig kurs:
			// måste man även kontrollera överlapp mot moduler i databasen (inte bara inom en request).

			// TODO: Bryt ut mappningslogik till en separat mappningsklass som har ansvar för att ta en Course till en CourseDto.
			// Tex courseMapper.GetCourseDto(course); som automapper, men man mappar själv och har kontroll på vad som sker.
			// Återanvändningsbar och om logiken förändras har man en single source of truth.

			// Skapa ny Course-entitet
			var course = new Course {
				Name = courseCreateDto.Name.Trim(),          // Trim undviker whitespace-problem i DB
				Description = courseCreateDto.Description.Trim(),
				StartDate = courseCreateDto.StartDate,
				EndDate = courseCreateDto.EndDate
			};

			// Mappa och koppla moduler till kursen
			foreach (var module in modules) {
				course.Modules.Add(new Module {
					Name = module.Name.Trim(),
					Description = module.Description.Trim(),
					StartDate = module.StartDate,
					EndDate = module.EndDate,
					Course = course // Navigation property så EF förstår relationen

					// Activities skapas inte här, utan i en separat controller och endpoint för att lägga till aktiviteter i en modul
					//  - Create() i ModuleActivitiesController.
					// Annars måste hela objektgrafen (Course + Modules + Activities) skapas i en och samma request.
				});
			}

			// Sparar objektgrafen med Course + Modules (om moduler finns med i samma request)
			_unitOfWork.CourseRepository.Create(course);

			await _unitOfWork.CompleteAsync();

			// Returnera DTO med ev. moduler - mappning från entitet till DTO
			return new CourseDto {
				Id = course.Id,
				Name = course.Name,
				Description = course.Description,
				StartDate = course.StartDate,
				EndDate = course.EndDate,

				// TODO: Om listan blir stor i framtiden: pagination / lazy loading
				Modules = course.Modules.Select(m => new ModuleDto {
					Id = m.Id,
					Name = m.Name,
					Description = m.Description,
					StartDate = m.StartDate,
					EndDate = m.EndDate
				}).ToList()
			};
		}

		public async Task<CourseDto> UpdateCourseAsync(CourseUpdateDto courseUpdateDto)
		{
			var course = await _unitOfWork.CourseRepository.GetByIdAsync(courseUpdateDto.Id);

			if (course == null)
				throw new Exception("Course not found");

			if (courseUpdateDto.EndDate < courseUpdateDto.StartDate)
				throw new Exception("End date must not be earlier than start date");

			course.Name = courseUpdateDto.Name;
			course.Description = courseUpdateDto.Description;
			course.StartDate = courseUpdateDto.StartDate;
			course.EndDate = courseUpdateDto.EndDate;

			_unitOfWork.CourseRepository.Update(course);
			await _unitOfWork.CompleteAsync();

			return CourseMapper.ToBasicCourseDto(course);
		}

		public async Task<IEnumerable<ParticipantDto>> GetParticipantsForUserCourseAsync(string userId)
		{
			var users = await _unitOfWork.CourseRepository.GetParticipantsForUserCourseAsync(userId);

			return users.Select(u => new ParticipantDto {
				Id = u.Id,
				FullName = $"{u.FirstName} {u.LastName}",
				Email = u.Email!
			});
		}

		public async Task<CourseDto?> GetCourseByIdAsync(int courseId)
		{
			var course = await _unitOfWork.CourseRepository.GetCourseById(courseId);
			if (course == null)
				return null;
			return CourseMapper.ToDetailedCourseDto(course);
		}

		public async Task<int> GetCourseProgressAsync(string userId, int courseId)
		{
			// All activities for the course
			var activities = await _unitOfWork.ActivityRepository.GetByCourseIdAsync(courseId);

			var activityCount = activities.Count();
			if (activityCount == 0) return 0;

			// All submissions for the course
			var submissionActivities = await _unitOfWork.ActivityRepository.GetSubmissionActivitiesForCourseAsync(courseId);

			// All submissions by the user
			var submissions = await _unitOfWork.SubmissionRepository.GetByStudentIdAsync(userId);


			// All submission id's
			var submissionActivityIds = submissionActivities
				.Select(a => a.Id)
				.ToHashSet();

			// All submission id's by the user
			var submittedActivityIds = submissions
				.Select(s => s.ActivityId)
				.ToHashSet();

			// Current date
			var now = DateTime.UtcNow;

			var completedCount = activities.Count(activity => {
				var hasEnded = activity.EndTime <= now;
				var reqSubmission = submissionActivityIds.Contains(activity.Id);
				
				// If the activity require a submission it is completed only if a submission is made
				if (reqSubmission) {
					var hasSubmitted = submittedActivityIds.Contains(activity.Id);
					return hasSubmitted;
				}

				// No submission required, completed when it has ended
				return hasEnded;

			});

			// return whole percentage points
			return (int) Math.Round((double)completedCount / activityCount * 100);

		

		}
	}
}