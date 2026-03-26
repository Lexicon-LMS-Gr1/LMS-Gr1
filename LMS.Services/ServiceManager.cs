using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> authService;
    private readonly Lazy<ICourseService> courseService;
    public readonly Lazy<IModuleService> moduleService;
	private readonly Lazy<IUserManagementService> userManagementService;
    private readonly Lazy<IDashboardService> dashboardService;

    public IAuthService AuthService => authService.Value;
    public ICourseService CourseService => courseService.Value;
    public IModuleService ModuleService => moduleService.Value;
    public IUserManagementService UserManagementService => userManagementService.Value;
    public IDashboardService DashboardService => dashboardService.Value;

    public ServiceManager(
        Lazy<IAuthService> authService,
        Lazy<ICourseService> courseService,
        Lazy<IUserManagementService> userManagementService,
        Lazy<IDashboardService> dashboardService,
        Lazy<IModuleService> moduleService
		)
    {
        this.authService = authService;
        this.courseService = courseService;
        this.userManagementService = userManagementService;
        this.dashboardService = dashboardService;
        this.moduleService = moduleService;
	}
}