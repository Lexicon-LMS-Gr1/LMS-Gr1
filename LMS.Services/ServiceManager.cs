using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private readonly Lazy<IAuthService> authService;
    private readonly Lazy<ICourseService> courseService;
    private readonly Lazy<IUserManagementService> userManagementService;
    private readonly Lazy<IDashboardService> dashboardService;

    public IAuthService AuthService => authService.Value;
    public ICourseService CourseService => courseService.Value;
    public IUserManagementService UserManagementService => userManagementService.Value;
    public IDashboardService DashboardService => dashboardService.Value;

    public ServiceManager(
        Lazy<IAuthService> authService,
        Lazy<ICourseService> courseService,
        Lazy<IUserManagementService> userManagementService,
        Lazy<IDashboardService> dashboardService)
    {
        this.authService = authService;
        this.courseService = courseService;
        this.userManagementService = userManagementService;
        this.dashboardService = dashboardService;
    }
}