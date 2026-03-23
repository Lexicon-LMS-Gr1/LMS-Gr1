using LMS.Shared.DTOs.Course;

namespace LMS.Blazor.Client.Services;

public interface IApiService
{
    Task<T?> GetAsync<T>(string endpoint, CancellationToken ct = default);
    Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data, CancellationToken ct = default);
    Task<IEnumerable<CourseDto>> GetCoursesAsync();
    Task<CourseDto?> CreateCourseAsync(CourseCreateDto dto, CancellationToken ct = default);
}