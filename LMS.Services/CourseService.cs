using Domain.Contracts.Repositories;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.Course;
using LMS.Shared.DTOs.Module;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;

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

            return courses.Select(c => new CourseListDto {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                StudentCount = c.Students.Count,
                ModuleCount = c.Modules.Count
            });
        }



        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _unitOfWork.CourseRepository.GetAllAsync();

            return courses.Select(c => new CourseDto {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate
            });
        }
        public async Task<CourseDto?> GetCourseForUserAsync(string userId)
        {
            var course = await _unitOfWork.CourseRepository.GetCourseForUserAsync(userId);

            if (course == null)
                return null;

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
            foreach (var module in modules)
            {
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
                            $"Modulerna '{a.Name}' och '{b.Name}' överlappar varandra.");
                    }
                }
            }

            // TODO: Om man senare tillåter att lägga till moduler i en befintlig kurs:
            // måste man även kontrollera överlapp mot moduler i databasen (inte inom en request).

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

                    // TODO: När Activities införs:
                    // Validera aktiviteter (StartTime/EndTime/DueDate) och mappa in dem här
                });
            }

            // Sparar hela objektgrafen (Course + Modules)
            _unitOfWork.CourseRepository.Create(course);

            await _unitOfWork.CompleteAsync();

            // Returnera DTO med moduler och aktiviteter (om de finns) - mappning från entitet till DTO
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
                throw new Exception("Course not found");

            if (courseUpdateDto.EndDate < courseUpdateDto.StartDate)
                throw new Exception("End date must be after start date");

            course.Name = courseUpdateDto.Name;
            course.Description = courseUpdateDto.Description;
            course.StartDate = courseUpdateDto.StartDate;
            course.EndDate = courseUpdateDto.EndDate;

            _unitOfWork.CourseRepository.Update(course);
            await _unitOfWork.CompleteAsync();

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                StartDate = course.StartDate,
                EndDate = course.EndDate
            };
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
	
		public async Task<IEnumerable<ModuleDto>> GetModulesByCourseIdAsync(int courseId)
        {
            var course = await _unitOfWork.CourseRepository.GetCourseAsync(courseId);

			if (course == null)
				return Enumerable.Empty<ModuleDto>();

			var moduleDtos = course.Modules.Select(m => new ModuleDto {
				Id = m.Id,
				Name = m.Name,
				Description = m.Description,
				StartDate = m.StartDate,
				EndDate = m.EndDate,
                // not included in the use case
				Activities = new List<ActivityDto>()
			}).ToList();

			return moduleDtos;

		}
	}
}