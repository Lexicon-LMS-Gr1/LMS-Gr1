using Bogus;
using LMS.Infrastructure.Data;
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
		if (!env.IsDevelopment()) return;

		var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
		if (await context.Users.AnyAsync(cancellationToken)) return;

		userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		try {
			await AddRolesAsync();
			var activityTypes = await SeedActivityTypesAsync(context);
			var courses = await SeedCoursesAsync(context);
			await SeedUsersModulesAndActivitiesAsync(context, courses, activityTypes);

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
			if (await roleManager.RoleExistsAsync(roleName)) continue;

			var result = await roleManager.CreateAsync(new IdentityRole(roleName));
			if (!result.Succeeded)
				throw new Exception(string.Join("\n", result.Errors.Select(e => e.Description)));
		}
	}

	private async Task<List<ActivityType>> SeedActivityTypesAsync(ApplicationDbContext context)
	{
		if (await context.Set<ActivityType>().AnyAsync())
			return await context.Set<ActivityType>().ToListAsync();

		var activityTypes = new List<ActivityType>
		{
			new() { Name = "Lecture" },
			new() { Name = "Assignment" },
			new() { Name = "Workshop" },
			new() { Name = "Exam" }
		};

		context.Set<ActivityType>().AddRange(activityTypes);
		await context.SaveChangesAsync();

		return activityTypes;
	}

	private async Task<List<Course>> SeedCoursesAsync(ApplicationDbContext context)
	{
		if (await context.Courses.AnyAsync())
			return await context.Courses.ToListAsync();

		var today = DateTime.UtcNow.Date;

		var courses = new List<Course>
		{
			new()
			{
				Name = ".NET 2025",
				Description = "Backendutveckling",
				StartDate = today.AddMonths(-12),
				EndDate = today.AddMonths(-10)
			},
			new()
			{
				Name = "JavaScript 2025",
				Description = "Frontendutveckling",
				StartDate = today.AddMonths(-10),
				EndDate = today.AddMonths(-8)
			},
			new()
			{
				Name = "Databasdesign 2025",
				Description = "Databaser och SQL",
				StartDate = today.AddMonths(-8),
				EndDate = today.AddMonths(-6)
			},
			new()
			{
				Name = ".NET 2026",
				Description = "Webb och API",
				StartDate = today.AddDays(-20),
				EndDate = today.AddMonths(2)
			},
			new()
			{
				Name = "Cloud 2026",
				Description = "Azure och molnet",
				StartDate = today.AddMonths(2),
				EndDate = today.AddMonths(4)
			},
			new()
			{
				Name = "AI 2026",
				Description = "AI och maskininlärning",
				StartDate = today.AddMonths(4),
				EndDate = today.AddMonths(6)
			}
		};

		context.Courses.AddRange(courses);
		await context.SaveChangesAsync();

		return courses;
	}

	private async Task SeedUsersModulesAndActivitiesAsync(
		ApplicationDbContext context,
		List<Course> courses,
		List<ActivityType> activityTypes)
	{
		var today = DateTime.UtcNow.Date;
		var password = configuration["password"];
		ArgumentNullException.ThrowIfNull(password);

		foreach (var course in courses) {
			var isOngoing = course.StartDate <= today && course.EndDate >= today;

			if (isOngoing) {
				await CreateSpecificUserAsync(
					firstName: "Teacher",
					lastName: "Demo",
					email: "teacher@test.com",
					role: TeacherRole,
					courseId: course.Id);

				await CreateSpecificUserAsync(
					firstName: "Student",
					lastName: "Demo",
					email: "student@test.com",
					role: StudentRole,
					courseId: course.Id);
			} else {
				await CreateAndAssignUserAsync(TeacherRole, course.Id);
			}

			for (int i = 0; i < 19; i++) {
				await CreateAndAssignUserAsync(StudentRole, course.Id);
			}

			if (!isOngoing) {
				await CreateAndAssignUserAsync(StudentRole, course.Id);
			}

			var modules = await CreateModulesAsync(context, course, 5);

			foreach (var module in modules) {
				await CreateActivitiesAsync(context, module, activityTypes);
			}
		}

		await CreateUsersWithoutCourseAsync(2, TeacherRole);
		await CreateUsersWithoutCourseAsync(5, StudentRole);
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

	private async Task CreateUsersWithoutCourseAsync(int count, string role)
	{
		for (int i = 0; i < count; i++) {
			await CreateUserAsync(role);
		}
	}

	private async Task<List<Module>> CreateModulesAsync(ApplicationDbContext context, Course course, int count)
	{
		var moduleNames = new[]
		{
			"Introduktion",
			"Databasdesign",
			"Webbutveckling",
			"API-utveckling",
			"Autentisering"
		};

		var modules = new List<Module>();
		var totalDays = (course.EndDate - course.StartDate).Days;
		var daysPerModule = totalDays / count;

		for (int i = 0; i < count; i++) {
			var startDate = course.StartDate.AddDays(i * daysPerModule);
			var endDate = i == count - 1
				? course.EndDate
				: course.StartDate.AddDays((i + 1) * daysPerModule - 1);

			modules.Add(new Module {
				Name = moduleNames[i],
				Description = "Grundläggande moment",
				StartDate = startDate,
				EndDate = endDate,
				CourseId = course.Id,
				Course = course
			});
		}

		context.Modules.AddRange(modules);
		await context.SaveChangesAsync();

		return modules;
	}

	private async Task<List<Activity>> CreateActivitiesAsync(
		ApplicationDbContext context,
		Module module,
		List<ActivityType> activityTypes)
	{
		var names = new[]
		{
			"Introduktion",
			"Workshop",
			"Inlämning",
			"Examination"
		};

		var activities = new List<Activity>();

		for (int i = 0; i < activityTypes.Count; i++) {
			var type = activityTypes[i];
			var day = module.StartDate.AddDays(i);

			var startTime = day.AddHours(9);
			var endTime = day.AddHours(11);

			activities.Add(new Activity {
				Name = names[i],
				Description = "Planerat moment",
				StartTime = startTime,
				EndTime = endTime,
				DueDate = type.Name == "Assignment" || type.Name == "Exam" ? endTime : null,
				ActivityTypeId = type.Id,
				ActivityType = type,
				ModuleId = module.Id,
				Module = module
			});
		}

		context.Set<Activity>().AddRange(activities);
		await context.SaveChangesAsync();

		return activities;
	}
}