using Bogus;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LMS.API.Services;





/*
 

BytMig123!

	Databasen har 3 kurser

				
				Databases 2026: januari - mars (har varit)
				Ansvarig lärare:	anna.teacher@test.com (gett feedback på alla inlämningar)
				Exempelelev:		alice@test.com (gjort alla inlämningar)

				.NET 2026: april - maj (pågående) 
				Ansvarig lärare:	teacher@test.com
				Exempelelev:		student@test.com

				Frontend 2026: maj - juli
				Ansvarig lärare:	maria.teacher@test.com
				Exempelelev:		charlie@test.com

 */








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

		try {
			await AddRolesAsync();
			var activityTypes = await SeedActivityTypesAsync(context);
			await SeedSpecialUsersAsync();
			await SeedAllAsync(context, activityTypes);

			logger.LogInformation("Seed complete");
		} catch (Exception ex) {
			logger.LogError(ex, "Data seed failed");
			throw;
		}
	}

	public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

	private async Task AddRolesAsync()
	{
		var roles = new[] { TeacherRole, StudentRole };

		foreach (var roleName in roles) {
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
			new() { Name = "Lecture" },
			new() { Name = "Assignment" },
			new() { Name = "Workshop" },
			new() { Name = "Exam" }
		};

		context.ActivityTypes.AddRange(activityTypes);
		await context.SaveChangesAsync();

		return activityTypes;
	}

	private async Task SeedSpecialUsersAsync()
	{
		if (seededStudents.Any() || seededTeachers.Any())
			return;

		seededStudents["alice"] = await CreateSpecificUserAsync("Alice", "Andersson", "alice@test.com", StudentRole);
		seededStudents["bob"] = await CreateSpecificUserAsync("Bob", "Berg", "student@test.com", StudentRole);
		seededStudents["charlie"] = await CreateSpecificUserAsync("Charlie", "Carlsson", "charlie@test.com", StudentRole);

		seededTeachers["anna"] = await CreateSpecificUserAsync("Anna", "Annason", "anna.teacher@test.com", TeacherRole);
		seededTeachers["erik"] = await CreateSpecificUserAsync("Erik", "Läraresson", "teacher@test.com", TeacherRole);
		seededTeachers["maria"] = await CreateSpecificUserAsync("Maria", "Mariasson", "maria.teacher@test.com", TeacherRole);
		seededTeachers["johan"] = await CreateSpecificUserAsync("Johan", "Johansson", "johan.teacher@test.com", TeacherRole);
		seededTeachers["sara"] = await CreateSpecificUserAsync("Sara", "Sarasson", "sara.teacher@test.com", TeacherRole);
	}

	private async Task SeedAllAsync(ApplicationDbContext context, List<ActivityType> activityTypes)
	{
		var lecture = activityTypes.First(x => x.Name == "Lecture");
		var assignment = activityTypes.First(x => x.Name == "Assignment");
		var workshop = activityTypes.First(x => x.Name == "Workshop");
		var exam = activityTypes.First(x => x.Name == "Exam");

		await SeedDatabaseCourseAsync(context, lecture, assignment, workshop, exam);
		await SeedDotNetCourseAsync(context, lecture, assignment, workshop, exam);
		await SeedFrontendCourseAsync(context, lecture, assignment, workshop, exam);
	}

	private async Task SeedDatabaseCourseAsync(
		ApplicationDbContext context,
		ActivityType lecture,
		ActivityType assignment,
		ActivityType workshop,
		ActivityType exam)
	{
		var databaseCourse = new Course {
			Name = "Databases 2026",
			Description = "Relationsdatabaser, SQL, datamodellering och normalisering.",
			StartDate = new DateTime(2026, 1, 15),
			EndDate = new DateTime(2026, 3, 13)
		};

		context.Courses.Add(databaseCourse);
		await context.SaveChangesAsync();

		var module1 = CreateModule("Databasgrunder", "Introduktion till databaser, tabeller, nycklar och SQL-grunder.", new DateTime(2026, 1, 15), new DateTime(2026, 1, 28), databaseCourse);
		var module2 = CreateModule("SQL Queries", "SELECT, JOIN, WHERE, GROUP BY och aggregatfunktioner.", new DateTime(2026, 1, 29), new DateTime(2026, 2, 11), databaseCourse);
		var module3 = CreateModule("Datamodellering", "ER-diagram, relationer och normalisering.", new DateTime(2026, 2, 12), new DateTime(2026, 2, 25), databaseCourse);
		var module4 = CreateModule("Avancerad SQL", "Subqueries, vyer, index och optimering.", new DateTime(2026, 2, 26), new DateTime(2026, 3, 13), databaseCourse);

		context.Modules.AddRange(module1, module2, module3, module4);
		await context.SaveChangesAsync();

		var m1Assignment1 = CreateActivity("Assignment 1: Tabeller och nycklar", "Skapa enkla tabeller och definiera primärnycklar.", Dt(2026, 1, 20), Dt(2026, 1, 20, 17), assignment, module1, Dt(2026, 1, 20, 17));
		var m1Assignment2 = CreateActivity("Assignment 2: Grundläggande SQL", "Skriv enkla INSERT-, UPDATE- och SELECT-frågor.", Dt(2026, 1, 28), Dt(2026, 1, 28, 17), assignment, module1, Dt(2026, 1, 28, 17));

		var m2Assignment1 = CreateActivity("Assignment 1: JOIN och filtrering", "Arbeta med JOIN, WHERE och ORDER BY.", Dt(2026, 2, 3), Dt(2026, 2, 3, 17), assignment, module2, Dt(2026, 2, 3, 17));
		var m2Assignment2 = CreateActivity("Assignment 2: Aggregat och gruppering", "Använd GROUP BY, HAVING och aggregatfunktioner.", Dt(2026, 2, 11), Dt(2026, 2, 11, 17), assignment, module2, Dt(2026, 2, 11, 17));

		var m3Assignment1 = CreateActivity("Assignment 1: ER-diagram", "Modellera ett system med entiteter och relationer.", Dt(2026, 2, 17), Dt(2026, 2, 17, 17), assignment, module3, Dt(2026, 2, 17, 17));
		var m3Assignment2 = CreateActivity("Assignment 2: Normalisering", "Normalisera en datamodell till minst tredje normalformen.", Dt(2026, 2, 25), Dt(2026, 2, 25, 17), assignment, module3, Dt(2026, 2, 25, 17));

		var m4Assignment1 = CreateActivity("Assignment 1: Vyer och subqueries", "Skapa vyer och använd subqueries i SQL.", Dt(2026, 3, 4), Dt(2026, 3, 4, 17), assignment, module4, Dt(2026, 3, 4, 17));
		var m4Assignment2 = CreateActivity("Assignment 2: SQL-optimering", "Analysera queries och föreslå förbättringar.", Dt(2026, 3, 12), Dt(2026, 3, 12, 17), assignment, module4, Dt(2026, 3, 12, 17));

		context.Activities.AddRange(
			CreateActivity("Lecture: Introduktion till databaser", "Översikt av databaser och hur de används.", Dt(2026, 1, 15), Dt(2026, 1, 16, 17), lecture, module1),
			CreateActivity("Workshop: Tabeller och relationer", "Praktisk modellering av tabeller och relationer.", Dt(2026, 1, 19), Dt(2026, 1, 19, 17), workshop, module1),
			m1Assignment1,
			CreateActivity("Lecture: SQL grunder", "SELECT, INSERT, UPDATE och DELETE.", Dt(2026, 1, 21), Dt(2026, 1, 23, 17), lecture, module1),
			CreateActivity("Workshop: SQL-labb", "Övningar med grundläggande SQL-frågor.", Dt(2026, 1, 26), Dt(2026, 1, 27, 17), workshop, module1),
			m1Assignment2,

			CreateActivity("Lecture: JOIN och filter", "Koppla tabeller och filtrera resultat.", Dt(2026, 1, 29), Dt(2026, 1, 30, 17), lecture, module2),
			CreateActivity("Workshop: Query-labb", "Praktiska övningar i SQL queries.", Dt(2026, 2, 2), Dt(2026, 2, 2, 17), workshop, module2),
			m2Assignment1,
			CreateActivity("Lecture: Gruppfunktioner", "SUM, COUNT, AVG, GROUP BY och HAVING.", Dt(2026, 2, 4), Dt(2026, 2, 6, 17), lecture, module2),
			CreateActivity("Workshop: Rapportfrågor", "Bygg queries för rapporter och sammanställningar.", Dt(2026, 2, 9), Dt(2026, 2, 10, 17), workshop, module2),
			m2Assignment2,

			CreateActivity("Lecture: ER-modellering", "Identifiera entiteter, attribut och relationer.", Dt(2026, 2, 12), Dt(2026, 2, 13, 17), lecture, module3),
			CreateActivity("Workshop: Modellering", "Skapa ER-diagram för ett domänproblem.", Dt(2026, 2, 16), Dt(2026, 2, 16, 17), workshop, module3),
			m3Assignment1,
			CreateActivity("Lecture: Normalisering", "1NF, 2NF och 3NF.", Dt(2026, 2, 18), Dt(2026, 2, 20, 17), lecture, module3),
			CreateActivity("Workshop: Datamodells-labb", "Förbättra och normalisera datamodeller.", Dt(2026, 2, 23), Dt(2026, 2, 24, 17), workshop, module3),
			m3Assignment2,

			CreateActivity("Lecture: Avancerad SQL", "Subqueries, CTE och vyer.", Dt(2026, 2, 26), Dt(2026, 2, 27, 17), lecture, module4),
			CreateActivity("Workshop: Query-optimering", "Analysera exekvering och förbättra queries.", Dt(2026, 3, 2), Dt(2026, 3, 3, 17), workshop, module4),
			m4Assignment1,
			CreateActivity("Lecture: Index och prestanda", "Hur index påverkar prestanda.", Dt(2026, 3, 5), Dt(2026, 3, 6, 17), lecture, module4),
			CreateActivity("Workshop: Prestandalabb", "Optimera queries och jämför resultat.", Dt(2026, 3, 9), Dt(2026, 3, 11, 17), workshop, module4),
			m4Assignment2,
			CreateActivity("Exam: Databases", "Avslutande examination för kursen.", Dt(2026, 3, 13), Dt(2026, 3, 13, 17), exam, module4, Dt(2026, 3, 13, 17))
		);

		await context.SaveChangesAsync();

		await CreateRandomStudentsForCourseAsync(databaseCourse.Id, 29);

		seededStudents["alice"].CourseId = databaseCourse.Id;
		var aliceUpdateResult = await userManager.UpdateAsync(seededStudents["alice"]);
		if (!aliceUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", aliceUpdateResult.Errors.Select(e => e.Description)));

		seededTeachers["anna"].CourseId = databaseCourse.Id;
		var annaUpdateResult = await userManager.UpdateAsync(seededTeachers["anna"]);
		if (!annaUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", annaUpdateResult.Errors.Select(e => e.Description)));

		context.Submissions.AddRange(
			CreateSubmissionWithFeedback(
				m1Assignment1.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m1-assignment1.pdf",
				"databases-m1-assignment1.pdf",
				"Min lösning på tabeller och nycklar.",
				Dt(2026, 1, 20, 14),
				"Bra struktur och korrekt användning av primärnycklar.",
				Dt(2026, 1, 21, 9),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m1Assignment2.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m1-assignment2.pdf",
				"databases-m1-assignment2.pdf",
				"Min lösning på grundläggande SQL.",
				Dt(2026, 1, 28, 15),
				"Bra SQL-frågor. Tänk på konsekvent formattering.",
				Dt(2026, 1, 29, 10),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m2Assignment1.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m2-assignment1.pdf",
				"databases-m2-assignment1.pdf",
				"JOIN och filtrering för rapportdelen.",
				Dt(2026, 2, 3, 14),
				"Väl fungerande joins och tydlig filtrering.",
				Dt(2026, 2, 4, 9),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m2Assignment2.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m2-assignment2.pdf",
				"databases-m2-assignment2.pdf",
				"Aggregat och gruppering enligt uppgiften.",
				Dt(2026, 2, 11, 14),
				"Bra användning av GROUP BY och HAVING.",
				Dt(2026, 2, 12, 10),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m3Assignment1.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m3-assignment1.pdf",
				"databases-m3-assignment1.pdf",
				"ER-diagram för bokningssystem.",
				Dt(2026, 2, 17, 13),
				"Tydligt diagram och korrekta relationer.",
				Dt(2026, 2, 18, 9),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m3Assignment2.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m3-assignment2.pdf",
				"databases-m3-assignment2.pdf",
				"Normalisering till tredje normalformen.",
				Dt(2026, 2, 25, 15),
				"Bra resonemang och korrekt normalisering.",
				Dt(2026, 2, 26, 10),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m4Assignment1.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m4-assignment1.pdf",
				"databases-m4-assignment1.pdf",
				"Vyer och subqueries för analysfrågor.",
				Dt(2026, 3, 4, 14),
				"Bra lösning. Subqueries används korrekt.",
				Dt(2026, 3, 5, 9),
				seededTeachers["anna"].Id),

			CreateSubmissionWithFeedback(
				m4Assignment2.Id,
				seededStudents["alice"].Id,
				"submissions/alice/databases-m4-assignment2.pdf",
				"databases-m4-assignment2.pdf",
				"Förslag på SQL-optimering och index.",
				Dt(2026, 3, 12, 15),
				"Väl motiverade förbättringar och bra analys.",
				Dt(2026, 3, 13, 10),
				seededTeachers["anna"].Id)
		);

		await context.SaveChangesAsync();
	}

	private async Task SeedDotNetCourseAsync(
		ApplicationDbContext context,
		ActivityType lecture,
		ActivityType assignment,
		ActivityType workshop,
		ActivityType exam)
	{
		var dotNetCourse = new Course {
			Name = ".NET 2026",
			Description = "Backendutveckling med C#, ASP.NET Core, Entity Framework och Web API.",
			StartDate = new DateTime(2026, 3, 15),
			EndDate = new DateTime(2026, 5, 13)
		};

		context.Courses.Add(dotNetCourse);
		await context.SaveChangesAsync();

		var module1 = CreateModule(".NET och C# grunder", "C# syntax, typer, klasser och objektorientering.", new DateTime(2026, 3, 15), new DateTime(2026, 3, 27), dotNetCourse);
		var module2 = CreateModule("ASP.NET Core", "Controllers, routing, dependency injection och middleware.", new DateTime(2026, 3, 28), new DateTime(2026, 4, 9), dotNetCourse);
		var module3 = CreateModule("Entity Framework Core", "Databas, modeller, relationer och queries med EF Core.", new DateTime(2026, 4, 10), new DateTime(2026, 4, 30), dotNetCourse);
		var module4 = CreateModule("Web API", "Bygga och dokumentera API:er med ASP.NET Core.", new DateTime(2026, 5, 1), new DateTime(2026, 5, 13), dotNetCourse);

		context.Modules.AddRange(module1, module2, module3, module4);
		await context.SaveChangesAsync();

		var m1Assignment1 = CreateActivity("Assignment 1: C# grunder", "Övningar i variabler, metoder och klasser.", Dt(2026, 3, 19), Dt(2026, 3, 19, 17), assignment, module1, Dt(2026, 3, 19, 17));
		var m1Assignment2 = CreateActivity("Assignment 2: Objektorienterad modell", "Bygg en enkel modell med klasser och arv.", Dt(2026, 3, 27), Dt(2026, 3, 27, 17), assignment, module1, Dt(2026, 3, 27, 17));

		var m2Assignment1 = CreateActivity("Assignment 1: Första MVC/API-projektet", "Skapa endpoints och testa routing.", Dt(2026, 4, 2), Dt(2026, 4, 2, 17), assignment, module2, Dt(2026, 4, 2, 17));
		var m2Assignment2 = CreateActivity("Assignment 2: ASP.NET Core-applikation", "Bygg en mindre applikation med DI och middleware.", Dt(2026, 4, 9), Dt(2026, 4, 9, 17), assignment, module2, Dt(2026, 4, 9, 17));

		var m3Assignment1 = CreateActivity("Assignment 1: Datamodell i EF Core", "Skapa modeller, relationer och migrationer.", Dt(2026, 4, 12), Dt(2026, 4, 12, 17), assignment, module3, Dt(2026, 4, 12, 17));
		var m3Assignment2 = CreateActivity("Assignment 2: Queries och persistens", "Arbeta med queries, include och sparande av data.", Dt(2026, 4, 14), Dt(2026, 4, 14, 17), assignment, module3, Dt(2026, 4, 14, 17));
		var m3Assignment3 = CreateActivity("Assignment 3: Persistens och relationer", "Fördjupning i relationer, queries och uppdateringar.", Dt(2026, 4, 30), Dt(2026, 4, 30, 17), assignment, module3, Dt(2026, 4, 30, 17));

		var m4Assignment1 = CreateActivity("Assignment 1: REST-endpoints", "Implementera CRUD-endpoints i ett Web API.", Dt(2026, 5, 6), Dt(2026, 5, 6, 17), assignment, module4, Dt(2026, 5, 6, 17));
		var m4Assignment2 = CreateActivity("Assignment 2: API-dokumentation", "Dokumentera och kvalitetssäkra API med Swagger.", Dt(2026, 5, 12), Dt(2026, 5, 12, 17), assignment, module4, Dt(2026, 5, 12, 17));

		context.Activities.AddRange(
			CreateActivity("Lecture: Introduktion till .NET", "Översikt av plattformen och kursupplägget.", Dt(2026, 3, 15), Dt(2026, 3, 16, 17), lecture, module1),
			CreateActivity("Workshop: C# syntax", "Praktiska övningar i syntax och kontrollflöden.", Dt(2026, 3, 17), Dt(2026, 3, 18, 17), workshop, module1),
			m1Assignment1,
			CreateActivity("Lecture: Klasser och objekt", "Objektorientering i C#.", Dt(2026, 3, 22), Dt(2026, 3, 23, 17), lecture, module1),
			CreateActivity("Workshop: Arv och interfaces", "Praktiskt arbete med arv, interface och abstraktion.", Dt(2026, 3, 24), Dt(2026, 3, 25, 17), workshop, module1),
			CreateActivity("Lecture: Repetition C# grunder", "Sammanfattning och förberedelse inför inlämning.", Dt(2026, 3, 26), Dt(2026, 3, 26, 17), lecture, module1),
			m1Assignment2,

			CreateActivity("Lecture: ASP.NET Core intro", "Projektstruktur, startup och grundläggande begrepp.", Dt(2026, 3, 28), Dt(2026, 3, 29, 17), lecture, module2),
			CreateActivity("Workshop: Routing och controllers", "Bygg controllers och arbeta med routes.", Dt(2026, 3, 30), Dt(2026, 4, 1, 17), workshop, module2),
			m2Assignment1,
			CreateActivity("Lecture: Dependency Injection", "Hur DI fungerar i ASP.NET Core.", Dt(2026, 4, 4), Dt(2026, 4, 5, 17), lecture, module2),
			CreateActivity("Workshop: Middleware", "Bygg och konfigurera middleware i pipeline.", Dt(2026, 4, 6), Dt(2026, 4, 7, 17), workshop, module2),
			CreateActivity("Lecture: Repetition ASP.NET Core", "Sammanfattning och förberedelse inför inlämning.", Dt(2026, 4, 8), Dt(2026, 4, 8, 17), lecture, module2),
			m2Assignment2,

			CreateActivity("Lecture: EF Core intro", "DbContext, entities och migrations.", Dt(2026, 4, 10), Dt(2026, 4, 11, 17), lecture, module3),
			m3Assignment1,
			CreateActivity("Workshop: Databas och migrationer", "Skapa databas och kör migrationer.", Dt(2026, 4, 13), Dt(2026, 4, 13, 17), workshop, module3),
			m3Assignment2,
			CreateActivity("Lecture: Relationer och queries", "En-till-många, include och filtrering.", Dt(2026, 4, 15), Dt(2026, 4, 17, 17), lecture, module3),
			CreateActivity("Workshop: Persistens", "Läsa, skriva och uppdatera data med EF Core.", Dt(2026, 4, 18), Dt(2026, 4, 21, 17), workshop, module3),
			CreateActivity("Lecture: Repetition EF Core", "Sammanfattning och förberedelse inför slutet av modulen.", Dt(2026, 4, 22), Dt(2026, 4, 24, 17), lecture, module3),
			CreateActivity("Workshop: Fördjupning i relationer", "Praktiskt arbete med relationer och uppdateringar.", Dt(2026, 4, 25), Dt(2026, 4, 28, 17), workshop, module3),
			CreateActivity("Lecture: Avslutande genomgång EF Core", "Sista genomgång innan sista inlämningen.", Dt(2026, 4, 29), Dt(2026, 4, 29, 17), lecture, module3),
			m3Assignment3,

			CreateActivity("Lecture: Web API intro", "REST, resurser och API-design.", Dt(2026, 5, 1), Dt(2026, 5, 2, 17), lecture, module4),
			CreateActivity("Workshop: CRUD-endpoints", "Implementera GET, POST, PUT och DELETE.", Dt(2026, 5, 3), Dt(2026, 5, 5, 17), workshop, module4),
			m4Assignment1,
			CreateActivity("Lecture: Swagger och dokumentation", "Dokumentera och testa API:er.", Dt(2026, 5, 7), Dt(2026, 5, 8, 17), lecture, module4),
			CreateActivity("Workshop: Validering och felhantering", "Förbättra API-kvalitet och robusthet.", Dt(2026, 5, 9), Dt(2026, 5, 11, 17), workshop, module4),
			m4Assignment2,
			CreateActivity("Exam: Web API", "Avslutande examination för kursen.", Dt(2026, 5, 13), Dt(2026, 5, 13, 17), exam, module4, Dt(2026, 5, 13, 17))
		);

		await context.SaveChangesAsync();

		await CreateRandomStudentsForCourseAsync(dotNetCourse.Id, 29);

		seededStudents["bob"].CourseId = dotNetCourse.Id;
		var bobUpdateResult = await userManager.UpdateAsync(seededStudents["bob"]);
		if (!bobUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", bobUpdateResult.Errors.Select(e => e.Description)));

		seededTeachers["erik"].CourseId = dotNetCourse.Id;
		var erikUpdateResult = await userManager.UpdateAsync(seededTeachers["erik"]);
		if (!erikUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", erikUpdateResult.Errors.Select(e => e.Description)));

		context.Submissions.AddRange(
			CreateSubmissionWithFeedback(
				m1Assignment1.Id,
				seededStudents["bob"].Id,
				"submissions/bob/dotnet-m1-assignment1.pdf",
				"dotnet-m1-assignment1.pdf",
				"Min lösning på C# grunder.",
				Dt(2026, 3, 19, 14),
				"Bra jobbat. Tydlig struktur och korrekt användning av klasser.",
				Dt(2026, 3, 20, 10),
				seededTeachers["erik"].Id),

			CreateSubmissionWithFeedback(
				m1Assignment2.Id,
				seededStudents["bob"].Id,
				"submissions/bob/dotnet-m1-assignment2.pdf",
				"dotnet-m1-assignment2.pdf",
				"Min objektorienterade modell.",
				Dt(2026, 3, 27, 15),
				"Bra uppdelning i klasser. Tänk på namngivning av properties.",
				Dt(2026, 3, 28, 9),
				seededTeachers["erik"].Id),

			CreateSubmissionWithFeedback(
				m2Assignment1.Id,
				seededStudents["bob"].Id,
				"submissions/bob/dotnet-m2-assignment1.pdf",
				"dotnet-m2-assignment1.pdf",
				"Mitt första MVC/API-projekt.",
				Dt(2026, 4, 2, 16),
				"Routing fungerar bra. Lägg till bättre felhantering.",
				Dt(2026, 4, 3, 11),
				seededTeachers["erik"].Id),
			/*
			CreateSubmissionWithFeedback(
				m2Assignment2.Id,
				seededStudents["bob"].Id,
				"submissions/bob/dotnet-m2-assignment2.pdf",
				"dotnet-m2-assignment2.pdf",
				"ASP.NET Core-applikation med middleware.",
				Dt(2026, 4, 9, 13),
				"Bra helhet. Dependency injection används korrekt.",
				Dt(2026, 4, 10, 10),
				seededTeachers["erik"].Id),
			*/
			CreateSubmissionWithFeedback(
				m3Assignment1.Id,
				seededStudents["bob"].Id,
				"submissions/bob/dotnet-m3-assignment1.pdf",
				"dotnet-m3-assignment1.pdf",
				"Datamodell och migrationer i EF Core.",
				Dt(2026, 4, 12, 14),
				"Relationerna är korrekta. Snygg lösning.",
				Dt(2026, 4, 13, 9),
				seededTeachers["erik"].Id),

			CreateSubmission(
				m3Assignment2.Id,
				seededStudents["bob"].Id,
				"submissions/bob/dotnet-m3-assignment2.pdf",
				"dotnet-m3-assignment2.pdf",
				"Queries och persistens med EF Core.",
				Dt(2026, 4, 14, 15))
		);

		await context.SaveChangesAsync();
	}

	private async Task SeedFrontendCourseAsync(
		ApplicationDbContext context,
		ActivityType lecture,
		ActivityType assignment,
		ActivityType workshop,
		ActivityType exam)
	{
		var frontendCourse = new Course {
			Name = "Frontend 2026",
			Description = "HTML, CSS, JavaScript och moderna frontend-flöden.",
			StartDate = new DateTime(2026, 5, 18),
			EndDate = new DateTime(2026, 7, 16)
		};

		context.Courses.Add(frontendCourse);
		await context.SaveChangesAsync();

		var module1 = CreateModule("HTML och CSS", "Grundläggande struktur, semantik, layout och responsiv design.", new DateTime(2026, 5, 18), new DateTime(2026, 6, 1), frontendCourse);
		var module2 = CreateModule("JavaScript grunder", "Variabler, funktioner, arrayer och objekt.", new DateTime(2026, 6, 2), new DateTime(2026, 6, 16), frontendCourse);
		var module3 = CreateModule("DOM och events", "Interaktion, eventhantering och formulär.", new DateTime(2026, 6, 17), new DateTime(2026, 7, 1), frontendCourse);
		var module4 = CreateModule("API och frontendprojekt", "Fetch, async/await och sammanhängande frontendprojekt.", new DateTime(2026, 7, 2), new DateTime(2026, 7, 16), frontendCourse);

		context.Modules.AddRange(module1, module2, module3, module4);
		await context.SaveChangesAsync();

		var m1Assignment1 = CreateActivity("Assignment 1: Semantisk HTML", "Bygg en semantisk webbsida med korrekt struktur.", Dt(2026, 5, 22), Dt(2026, 5, 22, 17), assignment, module1, Dt(2026, 5, 22, 17));
		var m1Assignment2 = CreateActivity("Assignment 2: Responsiv layout", "Skapa en responsiv layout med CSS.", Dt(2026, 6, 1), Dt(2026, 6, 1, 17), assignment, module1, Dt(2026, 6, 1, 17));

		var m2Assignment1 = CreateActivity("Assignment 1: Funktioner och data", "Arbeta med funktioner, arrayer och objekt.", Dt(2026, 6, 8), Dt(2026, 6, 8, 17), assignment, module2, Dt(2026, 6, 8, 17));
		var m2Assignment2 = CreateActivity("Assignment 2: JavaScript-övningar", "Lös flera uppgifter i JavaScript.", Dt(2026, 6, 16), Dt(2026, 6, 16, 17), assignment, module2, Dt(2026, 6, 16, 17));

		var m3Assignment1 = CreateActivity("Assignment 1: DOM manipulation", "Bygg interaktivitet med DOM API.", Dt(2026, 6, 23), Dt(2026, 6, 23, 17), assignment, module3, Dt(2026, 6, 23, 17));
		var m3Assignment2 = CreateActivity("Assignment 2: Eventdriven UI", "Hantera användarinteraktioner och formulär.", Dt(2026, 7, 1), Dt(2026, 7, 1, 17), assignment, module3, Dt(2026, 7, 1, 17));

		var m4Assignment1 = CreateActivity("Assignment 1: API-klient", "Hämta och visa data från ett API.", Dt(2026, 7, 9), Dt(2026, 7, 9, 17), assignment, module4, Dt(2026, 7, 9, 17));
		var m4Assignment2 = CreateActivity("Assignment 2: Frontendprojekt", "Bygg ett mindre frontendprojekt som slutuppgift.", Dt(2026, 7, 15), Dt(2026, 7, 15, 17), assignment, module4, Dt(2026, 7, 15, 17));

		context.Activities.AddRange(
			CreateActivity("Lecture: HTML intro", "Semantik, struktur och tillgänglighet.", Dt(2026, 5, 18), Dt(2026, 5, 19, 17), lecture, module1),
			CreateActivity("Workshop: CSS layout", "Box model, flexbox och grid.", Dt(2026, 5, 20), Dt(2026, 5, 21, 17), workshop, module1),
			m1Assignment1,
			CreateActivity("Lecture: Responsiv design", "Media queries och responsiva komponenter.", Dt(2026, 5, 25), Dt(2026, 5, 27, 17), lecture, module1),
			CreateActivity("Workshop: Layout-labb", "Bygg en responsiv sida från designskiss.", Dt(2026, 5, 28), Dt(2026, 5, 29, 17), workshop, module1),
			m1Assignment2,

			CreateActivity("Lecture: JavaScript intro", "Syntax, datatyper och kontrollflöden.", Dt(2026, 6, 2), Dt(2026, 6, 3, 17), lecture, module2),
			CreateActivity("Workshop: Funktioner och arrayer", "Praktiska övningar i JavaScript.", Dt(2026, 6, 4), Dt(2026, 6, 5, 17), workshop, module2),
			m2Assignment1,
			CreateActivity("Lecture: Objekt och iteration", "Objekt, loopar och arraymetoder.", Dt(2026, 6, 9), Dt(2026, 6, 11, 17), lecture, module2),
			CreateActivity("Workshop: JavaScript-labb", "Fördjupning i vardagliga JS-problem.", Dt(2026, 6, 12), Dt(2026, 6, 15, 17), workshop, module2),
			m2Assignment2,

			CreateActivity("Lecture: DOM intro", "Selektorer, noder och manipulering.", Dt(2026, 6, 17), Dt(2026, 6, 18, 17), lecture, module3),
			CreateActivity("Workshop: DOM-labb", "Bygg interaktiva komponenter med DOM.", Dt(2026, 6, 19), Dt(2026, 6, 22, 17), workshop, module3),
			m3Assignment1,
			CreateActivity("Lecture: Events", "Event listeners, bubbling och formulär.", Dt(2026, 6, 24), Dt(2026, 6, 26, 17), lecture, module3),
			CreateActivity("Workshop: UI-flöden", "Bygg eventdrivna användargränssnitt.", Dt(2026, 6, 29), Dt(2026, 6, 30, 17), workshop, module3),
			m3Assignment2,

			CreateActivity("Lecture: API och fetch", "Fetch, async/await och felhantering.", Dt(2026, 7, 2), Dt(2026, 7, 3, 17), lecture, module4),
			CreateActivity("Workshop: API-klient", "Bygg en klient som hämtar och visar data.", Dt(2026, 7, 6), Dt(2026, 7, 8, 17), workshop, module4),
			m4Assignment1,
			CreateActivity("Lecture: Projektstruktur", "Planera och strukturera frontendprojekt.", Dt(2026, 7, 10), Dt(2026, 7, 13, 17), lecture, module4),
			CreateActivity("Workshop: Projektarbete", "Praktiskt arbete med slutprojekt.", Dt(2026, 7, 14), Dt(2026, 7, 14, 17), workshop, module4),
			m4Assignment2,
			CreateActivity("Exam: Frontend", "Avslutande examination för kursen.", Dt(2026, 7, 16), Dt(2026, 7, 16, 17), exam, module4, Dt(2026, 7, 16, 17))
		);

		await context.SaveChangesAsync();

		await CreateRandomStudentsForCourseAsync(frontendCourse.Id, 29);

		seededStudents["charlie"].CourseId = frontendCourse.Id;
		var charlieUpdateResult = await userManager.UpdateAsync(seededStudents["charlie"]);
		if (!charlieUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", charlieUpdateResult.Errors.Select(e => e.Description)));

		seededTeachers["maria"].CourseId = frontendCourse.Id;
		var mariaUpdateResult = await userManager.UpdateAsync(seededTeachers["maria"]);
		if (!mariaUpdateResult.Succeeded)
			throw new Exception(string.Join("\n", mariaUpdateResult.Errors.Select(e => e.Description)));
	}

	private static Submission CreateSubmissionWithFeedback(
		int activityId,
		string studentId,
		string filePath,
		string fileName,
		string comment,
		DateTime submittedAt,
		string feedback,
		DateTime feedbackGivenAt,
		string feedbackGivenByTeacherId)
	{
		return new Submission {
			ActivityId = activityId,
			StudentId = studentId,
			FilePath = filePath,
			FileName = fileName,
			Comment = comment,
			SubmittedAt = submittedAt,
			Feedback = feedback,
			FeedbackGivenAt = feedbackGivenAt,
			FeedbackGivenByTeacherId = feedbackGivenByTeacherId
		};
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
		return new Module {
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
		return new Activity {
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

	private static Submission CreateSubmission(
		int activityId,
		string studentId,
		string filePath,
		string fileName,
		string comment,
		DateTime submittedAt)
	{
		return new Submission {
			ActivityId = activityId,
			StudentId = studentId,
			FilePath = filePath,
			FileName = fileName,
			Comment = comment,
			SubmittedAt = submittedAt
		};
	}

	private async Task CreateRandomStudentsForCourseAsync(int courseId, int count)
	{
		for (int i = 0; i < count; i++) {
			await CreateAndAssignUserAsync(StudentRole, courseId);
		}
	}

	private async Task<ApplicationUser> CreateUserAsync(string role)
	{
		var password = configuration["password"];
		ArgumentNullException.ThrowIfNull(password);

		var faker = new Faker<ApplicationUser>("sv")
			.RuleFor(u => u.FirstName, f => f.Name.FirstName())
			.RuleFor(u => u.LastName, f => f.Name.LastName())
			.RuleFor(u => u.Email, f => f.Internet.Email())
			.RuleFor(u => u.UserName, (f, u) => u.Email);

		var user = faker.Generate();

		var result = await userManager.CreateAsync(user, password);
		if (!result.Succeeded)
			throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));

		var roleResult = await userManager.AddToRoleAsync(user, role);
		if (!roleResult.Succeeded)
			throw new Exception(string.Join("\n", roleResult.Errors.Select(e => e.Description)));

		return user;
	}

	private async Task<ApplicationUser> CreateAndAssignUserAsync(string role, int courseId)
	{
		var user = await CreateUserAsync(role);
		user.CourseId = courseId;

		var updateResult = await userManager.UpdateAsync(user);
		if (!updateResult.Succeeded)
			throw new Exception(string.Join("\n", updateResult.Errors.Select(e => e.Description)));

		return user;
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

		var user = new ApplicationUser {
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
			throw new Exception(string.Join("\n", roleResult.Errors.Select(e => e.Description)));

		return user;
	}


}