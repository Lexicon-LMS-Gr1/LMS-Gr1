using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

//You need all this for JWT to work :)
//User Secrets Json
//Important to have secretkey inside same key "JwtSettings" as used in appsettings.json for get both sections!!!!
//{
//     "password": "YourSecretPasswordHere",
//     "JwtSettings": {
//        "secretkey": "ThisMustBeReallyLong!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"
//        }
//}
public class DataSeedHostingService : IHostedService
{
	private readonly IServiceProvider serviceProvider;
	private readonly IConfiguration configuration;
	private readonly ILogger<DataSeedHostingService> logger;
	private UserManager<ApplicationUser> userManager = null!;
	private RoleManager<IdentityRole> roleManager = null!;
	private const string TeacherRole = "Teacher";
	private const string StudentRole = "Student";

	public DataSeedHostingService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<DataSeedHostingService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.configuration = configuration;
		this.logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = serviceProvider.CreateScope();

		var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
		if (!env.IsDevelopment()) return;

		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		if (await context.Users.AnyAsync(cancellationToken)) return;

		userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		ArgumentNullException.ThrowIfNull(roleManager, nameof(roleManager));
		ArgumentNullException.ThrowIfNull(userManager, nameof(userManager));

		try {
			await AddRolesAsync([TeacherRole, StudentRole]);
			await SeedCourseWithTeacherStudentsModulesAndActivitiesAsync(context);
			logger.LogInformation("Seed complete");
		} catch (Exception ex) {
			logger.LogError($"Data seed fail with error: {ex.Message}");
			throw;
		}
	}

	private async Task AddRolesAsync(string[] rolenames)
	{
		foreach (string rolename in rolenames) {
			if (await roleManager.RoleExistsAsync(rolename)) continue;
			var role = new IdentityRole { Name = rolename };
			var res = await roleManager.CreateAsync(role);

			if (!res.Succeeded) throw new Exception(string.Join("\n", res.Errors));
		}
	}

	/*
	private async Task AddDemoUsersAsync()
	{
		var teacher = new ApplicationUser {
			UserName = "teacher@test.com",
			Email = "teacher@test.com"
		};

		var student = new ApplicationUser {
			UserName = "student@test.com",
			Email = "student@test.com"
		};

		await AddUserToDb([teacher, student]);

		var teacherRoleResult = await userManager.AddToRoleAsync(teacher, TeacherRole);
		if (!teacherRoleResult.Succeeded) throw new Exception(string.Join("\n", teacherRoleResult.Errors));

		var studentRoleResult = await userManager.AddToRoleAsync(student, StudentRole);
		if (!studentRoleResult.Succeeded) throw new Exception(string.Join("\n", studentRoleResult.Errors));
	}

	private async Task AddUsersAsync(int nrOfUsers)
	{
		var faker = new Faker<ApplicationUser>("sv").Rules((f, e) => {
			e.Email = f.Person.Email;
			e.UserName = f.Person.Email;
		});

		await AddUserToDb(faker.Generate(nrOfUsers));
	}

	private async Task AddUserToDb(IEnumerable<ApplicationUser> users)
	{
		var passWord = configuration["password"];
		ArgumentNullException.ThrowIfNull(passWord, nameof(passWord));

		foreach (var user in users) {
			var result = await userManager.CreateAsync(user, passWord);
			if (!result.Succeeded) throw new Exception(string.Join("\n", result.Errors));
		}
	}

	*/

	private async Task SeedCourseWithTeacherStudentsModulesAndActivitiesAsync(ApplicationDbContext context)
	{
		var password = configuration["password"];
		ArgumentNullException.ThrowIfNull(password);

		var courseStart = DateTime.UtcNow.Date;
		var courseEnd = courseStart.AddMonths(1);

		var lectureType = new ActivityType { Name = "Lecture" };
		var assignmentType = new ActivityType { Name = "Assignment" };
		var workshopType = new ActivityType { Name = "Workshop" };

		context.Set<ActivityType>().AddRange(lectureType, assignmentType, workshopType);
		await context.SaveChangesAsync();

		var course = new Course {
			Name = "Backendutveckling .NET",
			Description = "En enkel kurs i ASP.NET Core, EF Core och Web API.",
			StartDate = courseStart,
			EndDate = courseEnd
		};

		context.Courses.Add(course);
		await context.SaveChangesAsync();

		var teacherEmail = "teacher@test.com";
		var teacher = new ApplicationUser {
			FirstName = "Teacher",
			LastName = "Demo",
			Email = teacherEmail,
			UserName = teacherEmail,
			CourseId = course.Id
		};

		var teacherResult = await userManager.CreateAsync(teacher, password);
		if (!teacherResult.Succeeded)
			throw new Exception(string.Join("\n", teacherResult.Errors.Select(e => e.Description)));

		var teacherRoleResult = await userManager.AddToRoleAsync(teacher, TeacherRole);
		if (!teacherRoleResult.Succeeded)
			throw new Exception(string.Join("\n", teacherRoleResult.Errors.Select(e => e.Description)));

		var studentFaker = new Faker<ApplicationUser>("sv").Rules((f, u) => {
			var email = f.Internet.Email();
			u.FirstName = f.Name.FirstName();
			u.LastName = f.Name.LastName();
			u.Email = email;
			u.UserName = email;
			u.CourseId = course.Id;
		});

		var students = studentFaker.Generate(20);

		foreach (var student in students) {
			var createStudentResult = await userManager.CreateAsync(student, password);
			if (!createStudentResult.Succeeded)
				throw new Exception(string.Join("\n", createStudentResult.Errors.Select(e => e.Description)));

			var addStudentRoleResult = await userManager.AddToRoleAsync(student, StudentRole);
			if (!addStudentRoleResult.Succeeded)
				throw new Exception(string.Join("\n", addStudentRoleResult.Errors.Select(e => e.Description)));
		}

		var module1 = new Module {
			Name = "Intro till ASP.NET Core",
			Description = "Grundläggande om ASP.NET Core och projektstruktur.",
			StartDate = courseStart,
			EndDate = courseStart.AddDays(7),
			CourseId = course.Id,
			Course = course
		};

		var module2 = new Module {
			Name = "Web API",
			Description = "Controllers, endpoints och request/response.",
			StartDate = courseStart.AddDays(7),
			EndDate = courseStart.AddDays(14),
			CourseId = course.Id,
			Course = course
		};

		var module3 = new Module {
			Name = "Entity Framework Core",
			Description = "Databas, DbContext och relationer.",
			StartDate = courseStart.AddDays(14),
			EndDate = courseStart.AddDays(21),
			CourseId = course.Id,
			Course = course
		};

		var module4 = new Module {
			Name = "Autentisering och säkerhet",
			Description = "Identity, JWT och skydd av endpoints.",
			StartDate = courseStart.AddDays(21),
			EndDate = courseEnd,
			CourseId = course.Id,
			Course = course
		};

		context.Modules.AddRange(module1, module2, module3, module4);
		await context.SaveChangesAsync();

		var activities = new List<Activity>
		{
		new Activity
		{
			Name = "Introduktionsföreläsning",
			Description = "Genomgång av kursens upplägg och mål.",
			StartTime = module1.StartDate.AddHours(9),
			EndTime = module1.StartDate.AddHours(11),
			DueDate = null,
			ActivityTypeId = lectureType.Id,
			ActivityType = lectureType,
			ModuleId = module1.Id,
			Module = module1
		},
		new Activity
		{
			Name = "Installera utvecklingsmiljö",
			Description = "Installera Visual Studio, SQL Server och testa första projektet.",
			StartTime = module1.StartDate.AddDays(1).AddHours(10),
			EndTime = module1.StartDate.AddDays(1).AddHours(12),
			DueDate = module1.EndDate,
			ActivityTypeId = assignmentType.Id,
			ActivityType = assignmentType,
			ModuleId = module1.Id,
			Module = module1
		},
		new Activity
		{
			Name = "Skapa första API",
			Description = "Bygg ett enkelt API med en controller och några endpoints.",
			StartTime = module2.StartDate.AddHours(9),
			EndTime = module2.StartDate.AddHours(12),
			DueDate = null,
			ActivityTypeId = workshopType.Id,
			ActivityType = workshopType,
			ModuleId = module2.Id,
			Module = module2
		},
		new Activity
		{
			Name = "API-uppgift",
			Description = "Skapa CRUD-endpoints för en enkel resurs.",
			StartTime = module2.StartDate.AddDays(2).AddHours(13),
			EndTime = module2.StartDate.AddDays(2).AddHours(15),
			DueDate = module2.EndDate,
			ActivityTypeId = assignmentType.Id,
			ActivityType = assignmentType,
			ModuleId = module2.Id,
			Module = module2
		},
		new Activity
		{
			Name = "EF Core-föreläsning",
			Description = "Introduktion till migrationer, relationer och queries.",
			StartTime = module3.StartDate.AddHours(9),
			EndTime = module3.StartDate.AddHours(11),
			DueDate = null,
			ActivityTypeId = lectureType.Id,
			ActivityType = lectureType,
			ModuleId = module3.Id,
			Module = module3
		},
		new Activity
		{
			Name = "Databaslabb",
			Description = "Skapa modeller, migrationer och koppla till databas.",
			StartTime = module3.StartDate.AddDays(2).AddHours(10),
			EndTime = module3.StartDate.AddDays(2).AddHours(13),
			DueDate = module3.EndDate,
			ActivityTypeId = workshopType.Id,
			ActivityType = workshopType,
			ModuleId = module3.Id,
			Module = module3
		},
		new Activity
		{
			Name = "JWT-föreläsning",
			Description = "Genomgång av autentisering och auktorisering.",
			StartTime = module4.StartDate.AddHours(9),
			EndTime = module4.StartDate.AddHours(11),
			DueDate = null,
			ActivityTypeId = lectureType.Id,
			ActivityType = lectureType,
			ModuleId = module4.Id,
			Module = module4
		},
		new Activity
		{
			Name = "Säkerhetsuppgift",
			Description = "Skydda endpoints med JWT och roller.",
			StartTime = module4.StartDate.AddDays(2).AddHours(10),
			EndTime = module4.StartDate.AddDays(2).AddHours(12),
			DueDate = module4.EndDate,
			ActivityTypeId = assignmentType.Id,
			ActivityType = assignmentType,
			ModuleId = module4.Id,
			Module = module4
		}
	};

		context.Set<Activity>().AddRange(activities);
		await context.SaveChangesAsync();
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}