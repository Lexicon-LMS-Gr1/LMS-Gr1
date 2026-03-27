namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    ICourseService CourseService { get; }
    IModuleService ModuleService { get; }
    IUserManagementService UserManagementService { get; }
    IDashboardService DashboardService { get; }
    ITeacherDashboardService TeacherDashboardService { get; }
    IActivityService ActivityService { get; }
}