using Service.Contracts;

namespace LMS.Services;

public class ServiceManager : IServiceManager
{
    private Lazy<IAuthService> authService;
	private Lazy<ICourseService> courseService;
	private Lazy<IUserManagementService> userManagementService;

	public IAuthService AuthService => authService.Value;
    public ICourseService CourseService => courseService.Value;
    public IUserManagementService UserManagementService => userManagementService.Value;

	public ServiceManager(
		Lazy<IAuthService> authService,
		Lazy<ICourseService> courseService,
		Lazy<IUserManagementService> userManagementService)
    {
        this.authService = authService;
        this.courseService = courseService;
        this.userManagementService = userManagementService;
    }
}
