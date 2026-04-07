using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Services.Mappers;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;
using Microsoft.AspNetCore.Identity;
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
		private readonly IProgressService _progressService;
        private readonly IUserManagementService _userManagementService;
        public CourseService(
			IUnitOfWork unitOfWork,
			IProgressService progressService,
			IUserManagementService userManagementService)
        {
            _unitOfWork = unitOfWork;
            _progressService = progressService;
            _userManagementService = userManagementService;
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

			dto.Progress = await _progressService.GetCourseProgressAsync(userId, course.Id);
			foreach (var module in dto.Modules)
			{
				module.Progress = await _progressService.GetModuleProgressAsync(userId, module.Id);
			}

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
				throw new ArgumentException("Startdatum får inte vara senare än slutdatum.");

			// Modules är en lista (initierad till tom lista i DTOn)
			var modules = courseCreateDto.Modules;

			// Validera varje modul individuellt
			foreach (var module in modules)
			{
				// TODO: Delvis duplicering av DataAnnotations-validerigen. Ev centralisera denna validering senare.

				if (string.IsNullOrWhiteSpace(module.Name))
					throw new ArgumentException("En modul saknar namn.");

				if (string.IsNullOrWhiteSpace(module.Description))
					throw new ArgumentException($"Modul \"{module.Name}\" saknar beskrivning.");

				// Modulens startdatum <= slutdatum
				if (module.StartDate > module.EndDate)
					throw new ArgumentException($"Modul \"{module.Name}\" har ett startdatum som ligger efter slutdatum.");

				// Modul måste ligga inom kursens datumintervall
				if (module.StartDate < courseCreateDto.StartDate || module.EndDate > courseCreateDto.EndDate)
					throw new ArgumentException($"Modul \"{module.Name}\" har datum som ligger utanför kursens datum.");
			}

			// Kontrollera att moduler inte överlappar varandra (inom samma request)
			for (int i = 0; i < modules.Count; i++)
			{
				for (int j = i + 1; j < modules.Count; j++)
				{
					var a = modules[i];
					var b = modules[j];

					// Intervallöverlapp
					bool overlaps = a.StartDate <= b.EndDate && a.EndDate >= b.StartDate;

					if (overlaps)
					{
						throw new ArgumentException(
							$"Modulerna \"{a.Name}\" och \"{b.Name}\" överlappar varandra.");
					}
				}
			}

			// TODO: Om man senare tillåter att lägga till moduler i en befintlig kurs:
			// måste man även kontrollera överlapp mot moduler i databasen (inte bara inom en request).

			// TODO: Bryt ut mappningslogik till en separat mappningsklass som har ansvar för att ta en Course till en CourseDto.
			// Tex courseMapper.GetCourseDto(course); som automapper, men man mappar själv och har kontroll på vad som sker.
			// Återanvändningsbar och om logiken förändras har man en single source of truth.

			// Skapa ny Course-entitet
			var course = new Course
			{
				Name = courseCreateDto.Name.Trim(),          // Trim undviker whitespace-problem i DB
				Description = courseCreateDto.Description.Trim(),
				StartDate = courseCreateDto.StartDate,
				EndDate = courseCreateDto.EndDate
			};

			// Mappa och koppla moduler till kursen
			foreach (var module in modules)
			{
				course.Modules.Add(new Module
				{
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
			return new CourseDto
			{
				Id = course.Id,
				Name = course.Name,
				Description = course.Description,
				StartDate = course.StartDate,
				EndDate = course.EndDate,

				// TODO: Om listan blir stor i framtiden: pagination / lazy loading
				Modules = course.Modules.Select(m => new ModuleDto
				{
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
				throw new Exception("Kursen kunde inte hittas.");

            if (string.IsNullOrWhiteSpace(courseUpdateDto.Name))
                throw new ArgumentException("Kursnamn saknas.");

            if (string.IsNullOrWhiteSpace(courseUpdateDto.Description))
                throw new ArgumentException("Kursbeskrivning saknas.");

            if (courseUpdateDto.StartDate > courseUpdateDto.EndDate)
                throw new Exception("Startdatum får inte vara senare än slutdatum.");

            if (course.Modules.Count != 0)
            {
                // Tillåt inte ändring av kursdatum om kursen innehåller moduler, för moduldatumen kan då
                // hamna utanför kursdatumen.
                throw new Exception("Start- och slutdatum får inte ändras på kurs som innehåller moduler.");

                /*
                // Alternativt: Validera varje modul individuellt.
                foreach (var module in course.Modules)
                {
                    // Modul måste ligga inom kursens datumintervall.
                    if (module.StartDate < courseUpdateDto.StartDate || module.EndDate > courseUpdateDto.EndDate)
                        throw new ArgumentException($"Modul \"{module.Name}\" har datum som ligger utanför kursens datum.");
                }
                */
            }

            course.Name = courseUpdateDto.Name.Trim();
			course.Description = courseUpdateDto.Description.Trim();
			course.StartDate = courseUpdateDto.StartDate;
			course.EndDate = courseUpdateDto.EndDate;

			_unitOfWork.CourseRepository.Update(course);
			await _unitOfWork.CompleteAsync();

			return CourseMapper.ToBasicCourseDto(course);
		}

        public async Task<bool> DeleteCourseAsync(int courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetCourseWithAllDataAsync(courseId);

            if (course == null)
                return false;

            await _userManagementService.DeleteStudentsByCourseAsync(courseId);

            foreach (var module in course.Modules.ToList())
            {
                foreach (var activity in module.Activities.ToList())
                {
                    _unitOfWork.ActivityRepository.Delete(activity);
                }

                _unitOfWork.ModuleRepository.Delete(module);
            }

            _unitOfWork.CourseRepository.Delete(course);

            await _unitOfWork.CompleteAsync();

            return true;
        }


        public async Task<IEnumerable<ParticipantDto>> GetParticipantsForUserCourseAsync(string userId)
		{
			var users = await _unitOfWork.CourseRepository.GetParticipantsForUserCourseAsync(userId);

			return users.Select(u => new ParticipantDto
			{
				Id = u.Id,
				FirstName = u.FirstName,
				LastName = u.LastName,
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
	}
}
