using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;

// teacher: mona.bergqvist@test.com / BytMig123!
// student: felicia.dahlberg@test.com

public class DataSeedHostingService : IHostedService
{
	private readonly IServiceProvider serviceProvider;
	private readonly IConfiguration configuration;
	private readonly ILogger<DataSeedHostingService> logger;

	private UserManager<ApplicationUser> userManager = null!;
	private RoleManager<IdentityRole> roleManager = null!;

	private const string TeacherRole = "Teacher";
	private const string StudentRole = "Student";

	private readonly Dictionary<string, ApplicationUser> seededStudents = new();
	private readonly Dictionary<string, ApplicationUser> seededTeachers = new();
	private readonly Dictionary<string, Course> seededCourses = new();
	private readonly Dictionary<string, Module> seededModules = new();
	private readonly Dictionary<string, Activity> seededActivities = new();
	private readonly Dictionary<string, Document> seededDocuments = new();
	private readonly Dictionary<string, Submission> seededSubmissions = new();

	private ApplicationUser DotNetCourseTeacher = null!;

	private static readonly (string F, string L, string E)[] Students =
	[
		("Felicia", "Dahlberg", "felicia.dahlberg@test.com"),
		("Alice", "Wikström", "alice.wikstrom@test.com"),
		("Agnes", "Bergman", "agnes.bergman@test.com"),
		("Pernilla", "Persson", "pernilla.persson@test.com"),
		("Ingrid", "Jonsson", "ingrid.jonsson@test.com"),
		("Moa", "Holmqvist", "moa.holmqvist@test.com"),
		("Rasmus", "Lundström", "rasmus.lundstrom@test.com"),
		("Helen", "Nyström", "helen.nystrom@test.com"),
		("Thomas", "Mohamed", "thomas.mohamed@test.com"),
		("Isak", "Persson", "isak.persson@test.com"),
		("Britt-Marie", "Henriksson", "britt-marie.henriksson@test.com"),
		("Pernilla", "Gustafsson", "pernilla.gustafsson@test.com"),
		("Anders", "Sandberg", "anders.sandberg@test.com"),
		("John", "Lindgren", "john.lindgren@test.com"),
		("Robin", "Lindholm", "robin.lindholm@test.com"),
		("Katarina", "Engström", "katarina.engstrom@test.com"),
		("Dennis", "Claesson", "dennis.claesson@test.com"),
		("Per", "Hassan", "per.hassan@test.com"),
		("Ann-Christin", "Åberg", "ann-christin.aberg@test.com"),
		("Nathalie", "Sjöberg", "nathalie.sjoberg@test.com"),
		("Jakob", "Danielsson", "jakob.danielsson@test.com"),
		("Agnes", "Johansson", "agnes.johansson@test.com"),
		("John", "Ekström", "john.ekstrom@test.com"),
		("Camilla", "Fredriksson", "camilla.fredriksson@test.com"),
		("Niklas", "Pettersson", "niklas.pettersson@test.com"),
		("Nathalie", "Eriksson", "nathalie.eriksson@test.com"),
		("Birgitta", "Söderberg", "birgitta.soderberg@test.com"),
		("Thomas", "Dahlberg", "thomas.dahlberg@test.com"),
		("Gunnel", "Falk", "gunnel.falk@test.com"),
		("Niklas", "Lindholm", "niklas.lindholm@test.com")
	];

	private sealed record SubmissionSeedItem(
		string Key,
		string ActivityKey,
		string FileName,
		string Comment,
		string? Feedback,
		DateTime SubmittedAt);

	public DataSeedHostingService(
		IServiceProvider serviceProvider,
		IConfiguration configuration,
		ILogger<DataSeedHostingService> logger)
	{
		this.serviceProvider = serviceProvider;
		this.configuration = configuration;
		this.logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = serviceProvider.CreateScope();

		var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
		if (!env.IsDevelopment())
			return;

		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		if (await context.Users.AnyAsync(cancellationToken))
			return;

		userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		try
		{
			await AddRolesAsync();
			var activityTypes = await SeedActivityTypesAsync(context);
			await SeedSpecialUsersAsync();
			await SeedAllAsync(context, activityTypes);

			logger.LogInformation("Seed complete");
		} catch (Exception ex)
		{
			logger.LogError(ex, "Data seed failed");
			throw;
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task AddRolesAsync()
	{
		var roles = new[] { TeacherRole, StudentRole };

		foreach (var roleName in roles)
		{
			if (await roleManager.RoleExistsAsync(roleName))
				continue;

			var result = await roleManager.CreateAsync(new IdentityRole(roleName));
			if (!result.Succeeded)
				throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));
		}
	}

	private async Task<List<ActivityType>> SeedActivityTypesAsync(ApplicationDbContext context)
	{
		if (await context.ActivityTypes.AnyAsync())
			return await context.ActivityTypes.ToListAsync();

		var activityTypes = new List<ActivityType>
		{
			new() { Name = "Föreläsning" },
			new() { Name = "Inlämning" },
			new() { Name = "Workshop" },
			new() { Name = "Examination" }
		};

		context.ActivityTypes.AddRange(activityTypes);
		await context.SaveChangesAsync();

		return activityTypes;
	}

	private async Task SeedSpecialUsersAsync()
	{
		if (seededStudents.Any() || seededTeachers.Any())
			return;

		seededTeachers["pontus"] = await CreateSpecificUserAsync("Pontus", "Nordström", "pontus.nordstrom@test.com", TeacherRole);
		seededTeachers["mona"] = await CreateSpecificUserAsync("Mona", "Bergqvist", "mona.bergqvist@test.com", TeacherRole);
		seededTeachers["isak"] = await CreateSpecificUserAsync("Isak", "Engström", "isak.engstrom@test.com", TeacherRole);

		DotNetCourseTeacher = seededTeachers["mona"];
	}

	private async Task SeedAllAsync(ApplicationDbContext context, List<ActivityType> activityTypes)
	{
		var lecture = activityTypes.First(x => x.Name == "Föreläsning");
		var assignment = activityTypes.First(x => x.Name == "Inlämning");
		var workshop = activityTypes.First(x => x.Name == "Workshop");
		var exam = activityTypes.First(x => x.Name == "Examination");

		await SeedDotNetCourseAsync(context, lecture, assignment, workshop, exam);
	}

	private async Task SeedDotNetCourseAsync(
		ApplicationDbContext context,
		ActivityType lecture,
		ActivityType assignment,
		ActivityType workshop,
		ActivityType exam)
	{
		var dotNetCourse = new Course
		{
			Name = ".NET 2026",
			Description = "Backendutveckling med C#, ASP.NET Core, Entity Framework och Web API.",
			StartDate = new DateTime(2026, 3, 15),
			EndDate = new DateTime(2026, 5, 13)
		};

		context.Courses.Add(dotNetCourse);
		await context.SaveChangesAsync();
		seededCourses["dotnet-2026"] = dotNetCourse;

		var module1 = CreateModule(
			".NET och C# grunder",
			"C# syntax, typer, klasser och objektorientering.",
			new DateTime(2026, 3, 15),
			new DateTime(2026, 3, 27),
			dotNetCourse);

		var module2 = CreateModule(
			"ASP.NET Core",
			"Controllers, routing, dependency injection och middleware.",
			new DateTime(2026, 3, 28),
			new DateTime(2026, 4, 9),
			dotNetCourse);

		var module3 = CreateModule(
			"Entity Framework Core",
			"Databas, modeller, relationer och queries med EF Core.",
			new DateTime(2026, 4, 10),
			new DateTime(2026, 4, 30),
			dotNetCourse);

		var module4 = CreateModule(
			"Web API",
			"Bygga och dokumentera API:er med ASP.NET Core.",
			new DateTime(2026, 5, 1),
			new DateTime(2026, 5, 13),
			dotNetCourse);

		context.Modules.AddRange(module1, module2, module3, module4);
		await context.SaveChangesAsync();

		seededModules["m1-dotnet-basics"] = module1;
		seededModules["m2-aspnet-core"] = module2;
		seededModules["m3-ef-core"] = module3;
		seededModules["m4-web-api"] = module4;

		var activities = new Dictionary<string, Activity>
		{
			["m1-lecture-intro-dotnet"] = CreateActivity($"{lecture.Name}: Introduktion till .NET", "Översikt av plattformen och kursupplägget.", Dt(2026, 3, 15), Dt(2026, 3, 16, 17), lecture, module1),
			["m1-workshop-csharp-syntax"] = CreateActivity($"{workshop.Name}: C# syntax", "Praktiska övningar i syntax och kontrollflöden.", Dt(2026, 3, 17), Dt(2026, 3, 18, 17), workshop, module1),
			["m1-assignment-1"] = CreateActivity($"{assignment.Name} 1: C# grunder", "Övningar i variabler, metoder och klasser.", Dt(2026, 3, 19), Dt(2026, 3, 19, 17), assignment, module1, Dt(2026, 3, 19, 17)),
			["m1-lecture-classes-objects"] = CreateActivity($"{lecture.Name}: Klasser och objekt", "Objektorientering i C#.", Dt(2026, 3, 22), Dt(2026, 3, 23, 17), lecture, module1),
			["m1-workshop-inheritance-interfaces"] = CreateActivity($"{workshop.Name}: Arv och interfaces", "Praktiskt arbete med arv, interface och abstraktion.", Dt(2026, 3, 24), Dt(2026, 3, 25, 17), workshop, module1),
			["m1-lecture-repetition"] = CreateActivity($"{lecture.Name}: Repetition C# grunder", "Sammanfattning och förberedelse inför inlämning.", Dt(2026, 3, 26), Dt(2026, 3, 26, 17), lecture, module1),
			["m1-assignment-2"] = CreateActivity($"{assignment.Name} 2: Objektorienterad modell", "Bygg en enkel modell med klasser och arv.", Dt(2026, 3, 27), Dt(2026, 3, 27, 17), assignment, module1, Dt(2026, 3, 27, 17)),

			["m2-lecture-intro"] = CreateActivity($"{lecture.Name}: ASP.NET Core introduktion", "Projektstruktur, startup och grundläggande begrepp.", Dt(2026, 3, 28), Dt(2026, 3, 29, 17), lecture, module2),
			["m2-workshop-routing-controllers"] = CreateActivity($"{workshop.Name}: Routing och controllers", "Bygg controllers och arbeta med routes.", Dt(2026, 3, 30), Dt(2026, 4, 1, 17), workshop, module2),
			["m2-assignment-1"] = CreateActivity($"{assignment.Name} 1: Första MVC/API-projektet", "Skapa endpoints och testa routing.", Dt(2026, 4, 2), Dt(2026, 4, 2, 17), assignment, module2, Dt(2026, 4, 2, 17)),
			["m2-lecture-di"] = CreateActivity($"{lecture.Name}: Dependency Injection", "Hur DI fungerar i ASP.NET Core.", Dt(2026, 4, 4), Dt(2026, 4, 5, 17), lecture, module2),
			["m2-assignment-middleware"] = CreateActivity($"{assignment.Name}: Middleware", "Bygg och konfigurera middleware i pipeline.", Dt(2026, 4, 6), Dt(2026, 4, 6, 17), assignment, module2, Dt(2026, 4, 6, 17)),
			["m2-lecture-repetition"] = CreateActivity($"{lecture.Name}: Repetition ASP.NET Core", "Sammanfattning och förberedelse inför inlämning.", Dt(2026, 4, 7), Dt(2026, 4, 8, 17), lecture, module2),
			["m2-assignment-2"] = CreateActivity($"{assignment.Name} 2: ASP.NET Core-applikation", "Bygg en mindre applikation med DI och middleware.", Dt(2026, 4, 9), Dt(2026, 4, 9, 17), assignment, module2, Dt(2026, 4, 9, 17)),

			["m3-lecture-intro"] = CreateActivity($"{lecture.Name}: EF Core introduktion", "DbContext, entities och migrations.", Dt(2026, 4, 10), Dt(2026, 4, 11, 17), lecture, module3),
			["m3-assignment-1"] = CreateActivity($"{assignment.Name} 1: Datamodell i EF Core", "Skapa modeller, relationer och migrationer.", Dt(2026, 4, 12), Dt(2026, 4, 12, 17), assignment, module3, Dt(2026, 4, 12, 17)),
			["m3-workshop-database-migrations"] = CreateActivity($"{workshop.Name}: Databas och migrationer", "Skapa databas och kör migrationer.", Dt(2026, 4, 13), Dt(2026, 4, 13, 17), workshop, module3),
			["m3-assignment-2"] = CreateActivity($"{assignment.Name} 2: Queries och persistens", "Arbeta med queries, include och sparande av data.", Dt(2026, 4, 14), Dt(2026, 4, 14, 17), assignment, module3, Dt(2026, 4, 14, 17)),
			["m3-lecture-relations-queries"] = CreateActivity($"{lecture.Name}: Relationer och queries", "En-till-många, include och filtrering.", Dt(2026, 4, 15), Dt(2026, 4, 17, 17), lecture, module3),
			["m3-workshop-persistence"] = CreateActivity($"{workshop.Name}: Persistens", "Läsa, skriva och uppdatera data med EF Core.", Dt(2026, 4, 18), Dt(2026, 4, 21, 17), workshop, module3),
			["m3-lecture-repetition"] = CreateActivity($"{lecture.Name}: Repetition EF Core", "Sammanfattning och förberedelse inför slutet av modulen.", Dt(2026, 4, 22), Dt(2026, 4, 24, 17), lecture, module3),
			["m3-workshop-advanced-relations"] = CreateActivity($"{workshop.Name}: Fördjupning i relationer", "Praktiskt arbete med relationer och uppdateringar.", Dt(2026, 4, 25), Dt(2026, 4, 28, 17), workshop, module3),
			["m3-lecture-final-review"] = CreateActivity($"{lecture.Name}: Avslutande genomgång EF Core", "Sista genomgång innan sista inlämningen.", Dt(2026, 4, 29), Dt(2026, 4, 29, 17), lecture, module3),
			["m3-assignment-3"] = CreateActivity($"{assignment.Name} 3: Persistens och relationer", "Fördjupning i relationer, queries och uppdateringar.", Dt(2026, 4, 30), Dt(2026, 4, 30, 17), assignment, module3, Dt(2026, 4, 30, 17)),

			["m4-lecture-intro"] = CreateActivity($"{lecture.Name}: Web API introduktion", "REST, resurser och API-design.", Dt(2026, 5, 1), Dt(2026, 5, 2, 17), lecture, module4),
			["m4-workshop-crud"] = CreateActivity($"{workshop.Name}: CRUD-endpoints", "Implementera GET, POST, PUT och DELETE.", Dt(2026, 5, 3), Dt(2026, 5, 5, 17), workshop, module4),
			["m4-assignment-1"] = CreateActivity($"{assignment.Name} 1: REST-endpoints", "Implementera CRUD-endpoints i ett Web API.", Dt(2026, 5, 6), Dt(2026, 5, 6, 17), assignment, module4, Dt(2026, 5, 6, 17)),
			["m4-lecture-swagger"] = CreateActivity($"{lecture.Name}: Swagger och dokumentation", "Dokumentera och testa API:er.", Dt(2026, 5, 7), Dt(2026, 5, 8, 17), lecture, module4),
			["m4-workshop-validation-errorhandling"] = CreateActivity($"{workshop.Name}: Validering och felhantering", "Förbättra API-kvalitet och robusthet.", Dt(2026, 5, 9), Dt(2026, 5, 11, 17), workshop, module4),
			["m4-assignment-2"] = CreateActivity($"{assignment.Name} 2: API-dokumentation", "Dokumentera och kvalitetssäkra API med Swagger.", Dt(2026, 5, 12), Dt(2026, 5, 12, 17), assignment, module4, Dt(2026, 5, 12, 17)),
			["m4-exam"] = CreateActivity($"{exam.Name}: Web API", "Avslutande examination för kursen.", Dt(2026, 5, 13), Dt(2026, 5, 13, 17), exam, module4, Dt(2026, 5, 13, 17))
		};

		context.Activities.AddRange(activities.Values);
		await context.SaveChangesAsync();

		foreach (var activity in activities)
			seededActivities[activity.Key] = activity.Value;

		await CreateStudentsForCourseAsync(dotNetCourse.Id);

		DotNetCourseTeacher.CourseId = dotNetCourse.Id;
		var monaUpdateResult = await userManager.UpdateAsync(DotNetCourseTeacher);
		if (!monaUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", monaUpdateResult.Errors.Select(e => e.Description)));

		await context.SaveChangesAsync();

		await SeedRelevantDocumentsAsync(context);
		await SeedSubmissionsForFirstStudentsUntilTodayAsync(context);
		await SeedJourney8ScenarioAsync(context);
	}

	private static DateTime Dt(int year, int month, int day, int hour = 8, int minute = 0)
		=> new(year, month, day, hour, minute, 0);

	private static Module CreateModule(
		string name,
		string description,
		DateTime startDate,
		DateTime endDate,
		Course course)
	{
		return new Module
		{
			Name = name,
			Description = description,
			StartDate = startDate,
			EndDate = endDate,
			CourseId = course.Id,
			Course = course
		};
	}

	private static Activity CreateActivity(
		string name,
		string description,
		DateTime startTime,
		DateTime endTime,
		ActivityType activityType,
		Module module,
		DateTime? dueDate = null)
	{
		return new Activity
		{
			Name = name,
			Description = description,
			StartTime = startTime,
			EndTime = endTime,
			DueDate = dueDate,
			ActivityTypeId = activityType.Id,
			ActivityType = activityType,
			ModuleId = module.Id,
			Module = module
		};
	}

	private async Task CreateStudentsForCourseAsync(int courseId)
	{
		foreach (var (firstName, lastName, email) in Students)
		{
			var student = await CreateSpecificUserAsync(firstName, lastName, email, StudentRole, courseId);
			seededStudents[email] = student;
		}
	}

	private async Task SeedRelevantDocumentsAsync(ApplicationDbContext context)
	{
		var teacher = DotNetCourseTeacher;
		var course = seededCourses["dotnet-2026"];
		var module1 = seededModules["m1-dotnet-basics"];
		var module2 = seededModules["m2-aspnet-core"];
		var module3 = seededModules["m3-ef-core"];
		var module4 = seededModules["m4-web-api"];
		var middlewareActivity = seededActivities["m2-assignment-middleware"];
		var efQueriesActivity = seededActivities["m3-assignment-2"];

		var documents = new Dictionary<string, Document>
		{
			["course-plan"] = CreateDocument(
				name: "Kursplan .NET 2026",
				description: "Översikt över kursmål, upplägg, examination och bedömning.",
				uploadTimestamp: new DateTime(2026, 3, 10, 9, 0, 0),
				filePath: "seed/course/kursplan-dotnet-2026.pdf",
				fileName: "kursplan-dotnet-2026.pdf",
				contentType: "application/pdf",
				fileSize: 184_320,
				uploadedByUserId: teacher.Id,
				courseId: course.Id),

			["course-schedule"] = CreateDocument(
				name: "Schema .NET 2026",
				description: "Kursens schema med moduler, workshops och inlämningar.",
				uploadTimestamp: new DateTime(2026, 3, 12, 10, 30, 0),
				filePath: "seed/course/schema-dotnet-2026.pdf",
				fileName: "schema-dotnet-2026.pdf",
				contentType: "application/pdf",
				fileSize: 142_220,
				uploadedByUserId: teacher.Id,
				courseId: course.Id),

			["course-links"] = CreateDocument(
				name: "Intressanta länkar för kursen",
				description: "Samlad lista med relevanta länkar till officiell dokumentation och vidare läsning för hela kursen.",
				uploadTimestamp: new DateTime(2026, 3, 18, 9, 45, 0),
				filePath: "seed/course/intressanta-lankar-dotnet-2026.pdf",
				fileName: "intressanta-lankar-dotnet-2026.pdf",
				contentType: "application/pdf",
				fileSize: 96_300,
				uploadedByUserId: teacher.Id,
				courseId: course.Id),

			["m1-reading-instructions"] = CreateDocument(
				name: "Läsanvisningar .NET och C# grunder",
				description: "Läsanvisningar för modul 1 med fokus på C# syntax, typer, klasser och objektorientering.",
				uploadTimestamp: new DateTime(2026, 3, 14, 8, 30, 0),
				filePath: "seed/modules/lasanvisningar-dotnet-csharp-grunder.pdf",
				fileName: "lasanvisningar-dotnet-csharp-grunder.pdf",
				contentType: "application/pdf",
				fileSize: 156_400,
				uploadedByUserId: teacher.Id,
				moduleId: module1.Id),

			["m1-extra-reading"] = CreateDocument(
				name: "Extra läsanvisningar C# och OOP",
				description: "Fördjupande material om klasser, arv, interface och god struktur i C#.",
				uploadTimestamp: new DateTime(2026, 3, 22, 13, 10, 0),
				filePath: "seed/modules/extra-lasanvisningar-csharp-oop.pdf",
				fileName: "extra-lasanvisningar-csharp-oop.pdf",
				contentType: "application/pdf",
				fileSize: 118_500,
				uploadedByUserId: teacher.Id,
				moduleId: module1.Id),

			["m2-reading-instructions"] = CreateDocument(
				name: "Läsanvisningar ASP.NET Core",
				description: "Läsanvisningar för modul 2 med fokus på controllers, routing, dependency injection och middleware.",
				uploadTimestamp: new DateTime(2026, 3, 27, 14, 0, 0),
				filePath: "seed/modules/lasanvisningar-aspnet-core.pdf",
				fileName: "lasanvisningar-aspnet-core.pdf",
				contentType: "application/pdf",
				fileSize: 221_300,
				uploadedByUserId: teacher.Id,
				moduleId: module2.Id),

			["m2-extra-reading"] = CreateDocument(
				name: "Extra läsanvisningar ASP.NET Core",
				description: "Fördjupande exempel på services, routing, request pipeline och middleware.",
				uploadTimestamp: new DateTime(2026, 4, 3, 13, 20, 0),
				filePath: "seed/modules/extra-lasanvisningar-aspnet-core.pdf",
				fileName: "extra-lasanvisningar-aspnet-core.pdf",
				contentType: "application/pdf",
				fileSize: 227_450,
				uploadedByUserId: teacher.Id,
				moduleId: module2.Id),

			["m3-reading-instructions"] = CreateDocument(
				name: "Läsanvisningar Entity Framework Core",
				description: "Läsanvisningar för modul 3 med fokus på relationer, migrationer, queries och persistens i EF Core.",
				uploadTimestamp: new DateTime(2026, 4, 10, 8, 30, 0),
				filePath: "seed/modules/lasanvisningar-ef-core.pdf",
				fileName: "lasanvisningar-ef-core.pdf",
				contentType: "application/pdf",
				fileSize: 231_100,
				uploadedByUserId: teacher.Id,
				moduleId: module3.Id),

			["m3-extra-reading"] = CreateDocument(
				name: "Extra läsanvisningar EF Core",
				description: "Exempel och fördjupning kring Include, queries, relationer och uppdatering av data.",
				uploadTimestamp: new DateTime(2026, 4, 11, 9, 5, 0),
				filePath: "seed/modules/extra-lasanvisningar-ef-core.pdf",
				fileName: "extra-lasanvisningar-ef-core.pdf",
				contentType: "application/pdf",
				fileSize: 173_900,
				uploadedByUserId: teacher.Id,
				moduleId: module3.Id),

			["m4-reading-instructions"] = CreateDocument(
				name: "Läsanvisningar Web API",
				description: "Läsanvisningar för modul 4 med fokus på REST, CRUD, dokumentation, validering och felhantering.",
				uploadTimestamp: new DateTime(2026, 4, 30, 10, 0, 0),
				filePath: "seed/modules/lasanvisningar-web-api.pdf",
				fileName: "lasanvisningar-web-api.pdf",
				contentType: "application/pdf",
				fileSize: 198_700,
				uploadedByUserId: teacher.Id,
				moduleId: module4.Id),

			["m4-links"] = CreateDocument(
				name: "Intressanta länkar Web API",
				description: "Samling länkar om REST-design, Swagger, validering och felhantering.",
				uploadTimestamp: new DateTime(2026, 5, 2, 11, 0, 0),
				filePath: "seed/modules/intressanta-lankar-web-api.pdf",
				fileName: "intressanta-lankar-web-api.pdf",
				contentType: "application/pdf",
				fileSize: 88_000,
				uploadedByUserId: teacher.Id,
				moduleId: module4.Id),

			["middleware-instructions"] = CreateDocument(
				name: "Instruktion Middleware",
				description: "Detaljerad instruktion för inlämningen om middleware och pipeline.",
				uploadTimestamp: new DateTime(2026, 4, 5, 11, 0, 0),
				filePath: "seed/activities/instruktion-middleware.pdf",
				fileName: "instruktion-middleware.pdf",
				contentType: "application/pdf",
				fileSize: 118_430,
				uploadedByUserId: teacher.Id,
				activityId: middlewareActivity.Id),

			["middleware-checklist"] = CreateDocument(
				name: "Checklista Middleware-uppgift",
				description: "Kort checklista för loggning, ordning i pipeline och felhantering inför inlämning.",
				uploadTimestamp: new DateTime(2026, 4, 6, 9, 0, 0),
				filePath: "seed/activities/checklista-middleware.pdf",
				fileName: "checklista-middleware.pdf",
				contentType: "application/pdf",
				fileSize: 64_200,
				uploadedByUserId: teacher.Id,
				activityId: middlewareActivity.Id),

			["efqueries-examples"] = CreateDocument(
				name: "Exempel på queries i EF Core",
				description: "Exempelmaterial till aktiviteten om Include, filtrering och persistens.",
				uploadTimestamp: new DateTime(2026, 4, 14, 9, 10, 0),
				filePath: "seed/activities/exempel-queries-efcore.pdf",
				fileName: "exempel-queries-efcore.pdf",
				contentType: "application/pdf",
				fileSize: 126_880,
				uploadedByUserId: teacher.Id,
				activityId: efQueriesActivity.Id)
		};

		context.Documents.AddRange(documents.Values);
		await context.SaveChangesAsync();

		foreach (var item in documents)
			seededDocuments[item.Key] = item.Value;
	}

	private async Task SeedSubmissionsForFirstStudentsUntilTodayAsync(ApplicationDbContext context)
	{
		var submissionPlan = new[]
		{
		new SubmissionSeedItem(
			Key: "m1-assignment-1",
			ActivityKey: "m1-assignment-1",
			FileName: "csharp-grunder.pdf",
			Comment: "Jag har gjort uppgifterna om variabler, metoder och klasser. Jag försökte skriva tydliga metoder och dela upp lösningen i mindre delar.",
			Feedback: "Bra struktur och tydliga metoder. Fortsätt tänka på namngivning och att hålla varje klass fokuserad på ett ansvar.",
			SubmittedAt: new DateTime(2026, 3, 19, 15, 10, 0)),

		new SubmissionSeedItem(
			Key: "m1-assignment-2",
			ActivityKey: "m1-assignment-2",
			FileName: "objektorienterad-modell.pdf",
			Comment: "Här har jag byggt en modell med klasser, arv och interfaces. Jag har försökt hålla isär ansvar mellan olika klasser i modellen.",
			Feedback: "Det här visar att du förstått grunderna i objektorientering. Nästa steg är att göra modellen ännu mer konsekvent i hur du använder interface och arv.",
			SubmittedAt: new DateTime(2026, 3, 27, 16, 5, 0)),

		new SubmissionSeedItem(
			Key: "m2-assignment-1",
			ActivityKey: "m2-assignment-1",
			FileName: "forsta-api-projektet.pdf",
			Comment: "Jag har skapat endpoints och testat routing i projektet. GET och POST fungerar och jag har även testat route-parametrar.",
			Feedback: "Bra första API-struktur. Jag ser att du förstått routing och controllers. Tänk även på validering av input i kommande uppgifter.",
			SubmittedAt: new DateTime(2026, 4, 2, 14, 45, 0)),

		new SubmissionSeedItem(
			Key: "m2-assignment-middleware",
			ActivityKey: "m2-assignment-middleware",
			FileName: "middleware.pdf",
			Comment: "Jag har byggt ett middleware för loggning och testat ordningen i pipeline. Det hjälpte mig att förstå request-flödet bättre.",
			Feedback: "Bra jobbat med pipeline-tänket. Nästa gång kan du gärna visa ännu tydligare hur du hanterar exceptions eller skriver mer riktad loggning.",
			SubmittedAt: new DateTime(2026, 4, 6, 15, 20, 0)),

		new SubmissionSeedItem(
			Key: "m2-assignment-2",
			ActivityKey: "m2-assignment-2",
			FileName: "aspnet-core-applikation.pdf",
			Comment: "Jag har byggt en mindre applikation med dependency injection och middleware. Jag börjar förstå hur services registreras och används.",
			Feedback: "Fin utveckling här. Du använder DI på ett vettigt sätt och applikationen är lättare att följa än tidigare lösningar.",
			SubmittedAt: new DateTime(2026, 4, 9, 16, 10, 0)),

		new SubmissionSeedItem(
			Key: "m3-assignment-1",
			ActivityKey: "m3-assignment-1",
			FileName: "datamodell-efcore.pdf",
			Comment: "Jag har skapat modeller, relationer och migrationer i EF Core. Det svåraste var att få relationerna rätt, men migrationerna fungerar nu.",
			Feedback: "Bra jobbat med modeller och migrationer. Relationerna fungerar överlag bra. Se gärna över navigation properties och foreign keys så att de blir ännu tydligare och mer konsekventa.",
			SubmittedAt: new DateTime(2026, 4, 12, 16, 4, 0))
	};

		var submissions = new List<Submission>();

		foreach (var (studentTuple, studentIndex) in Students.Take(4).Select((student, index) => (student, index)))
		{
			if (!seededStudents.TryGetValue(studentTuple.E, out var student))
				continue;

			foreach (var plan in submissionPlan)
			{
				var activity = seededActivities[plan.ActivityKey];
				var submittedAt = plan.SubmittedAt.AddMinutes(studentIndex * 7);
				var submissionKey = $"{student.Email}:{plan.Key}";

				var submission = new Submission
				{
					ActivityId = activity.Id,
					StudentId = student.Id,
					FilePath = $"seed/submissions/{student.Id}/{activity.Id}-{plan.FileName}",
					FileName = plan.FileName,
					Comment = $"{plan.Comment} / {student.FirstName} {student.LastName}",
					SubmittedAt = submittedAt,
					IsLate = activity.DueDate.HasValue && submittedAt > activity.DueDate.Value,
					Feedback = null,
					FeedbackGivenAt = null,
					FeedbackGivenByTeacherId = null
				};

				submissions.Add(submission);
				seededSubmissions[submissionKey] = submission;
			}
		}

		context.Submissions.AddRange(submissions);
		await context.SaveChangesAsync();

		foreach (var (studentTuple, _) in Students.Take(4).Select((student, index) => (student, index)))
		{
			if (!seededStudents.TryGetValue(studentTuple.E, out var student))
				continue;

			foreach (var plan in submissionPlan)
			{
				var submissionKey = $"{student.Email}:{plan.Key}";
				var submission = seededSubmissions[submissionKey];

				submission.Feedback = plan.Feedback;
				submission.FeedbackGivenAt = submission.SubmittedAt.AddDays(1).AddHours(2);
				submission.FeedbackGivenByTeacherId = DotNetCourseTeacher.Id;
			}
		}

		await context.SaveChangesAsync();
	}

	private async Task SeedJourney8ScenarioAsync(ApplicationDbContext context)
	{
		var teacher = DotNetCourseTeacher;
		var course = seededCourses["dotnet-2026"];
		var student = seededStudents["felicia.dahlberg@test.com"];
		var activity = seededActivities["m3-assignment-2"];

		var submissionKey = $"{student.Email}:journey8-m3-assignment-2";

		if (!seededSubmissions.TryGetValue(submissionKey, out var submission))
		{
			var submittedAt = new DateTime(2026, 4, 14, 10, 0, 0);

			submission = new Submission
			{
				ActivityId = activity.Id,
				StudentId = student.Id,
				FilePath = $"seed/submissions/{student.Id}/{activity.Id}-queries-och-persistens.pdf",
				FileName = "queries-och-persistens.pdf",
				Comment = "Jag har arbetat med queries, Include och att spara data i databasen. Jag testade både hämtning och uppdatering och fick bättre förståelse för relationer.",
				SubmittedAt = submittedAt,
				IsLate = activity.DueDate.HasValue && submittedAt > activity.DueDate.Value,
				Feedback = null,
				FeedbackGivenAt = null,
				FeedbackGivenByTeacherId = null
			};

			context.Submissions.Add(submission);
			await context.SaveChangesAsync();

			seededSubmissions[submissionKey] = submission;
		}

		var existingNotification = await context.Notifications.AnyAsync(n =>
			n.UserId == teacher.Id &&
			n.Type == NotificationType.SubmissionCreated &&
			n.SubmissionId == submission.Id);

		if (!existingNotification)
		{
			var notification = new Notification
			{
				UserId = teacher.Id,
				Type = NotificationType.SubmissionCreated,
				CreatedAt = submission.SubmittedAt,
				IsRead = false,
				ActorUserId = student.Id,
				ActorName = $"{student.FirstName} {student.LastName}",
				CourseId = course.Id,
				CourseName = course.Name,
				ModuleId = activity.ModuleId,
				ModuleName = activity.Module.Name,
				ActivityId = activity.Id,
				ActivityName = activity.Name,
				SubmissionId = submission.Id,
				Message = $"{student.FirstName} {student.LastName} lämnade in \"{activity.Name}\"."
			};

			context.Notifications.Add(notification);
			await context.SaveChangesAsync();
		}
	}

	private static Document CreateDocument(
		string name,
		string description,
		DateTime uploadTimestamp,
		string filePath,
		string fileName,
		string contentType,
		long fileSize,
		string uploadedByUserId,
		int? courseId = null,
		int? moduleId = null,
		int? activityId = null)
	{
		return new Document
		{
			Name = name,
			Description = description,
			UploadTimestamp = uploadTimestamp,
			FilePath = filePath,
			FileName = fileName,
			ContentType = contentType,
			FileSize = fileSize,
			UploadedByUserId = uploadedByUserId,
			CourseId = courseId,
			ModuleId = moduleId,
			ActivityId = activityId
		};
	}

	private async Task<ApplicationUser> CreateSpecificUserAsync(
		string firstName,
		string lastName,
		string email,
		string role,
		int? courseId = null)
	{
		var password = configuration["password"];
		ArgumentNullException.ThrowIfNull(password);

		var user = new ApplicationUser
		{
			FirstName = firstName,
			LastName = lastName,
			Email = email,
			UserName = email,
			CourseId = courseId
		};

		var result = await userManager.CreateAsync(user, password);
		if (!result.Succeeded)
			throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));

		var roleResult = await userManager.AddToRoleAsync(user, role);
		if (!roleResult.Succeeded)
			throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));

		return user;
	}
}