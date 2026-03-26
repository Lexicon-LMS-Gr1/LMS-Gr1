namespace LMS.Blazor.Client.Components.CourseOverview;

public sealed class CourseOverviewViewModel
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DateRange { get; init; } = string.Empty;
    public int Progress { get; init; }
    public IReadOnlyList<ModuleViewModel> Modules { get; init; } = [];
}

public sealed class ModuleViewModel
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DateRange { get; init; } = string.Empty;
    public int Progress { get; init; }
    public bool IsExpanded { get; set; }
    public IReadOnlyList<ActivityViewModel> Activities { get; init; } = [];
}

public sealed class ActivityViewModel
{
    public string Name { get; init; } = string.Empty;
    public string ActivityTypeName { get; init; } = string.Empty;
    public string DateRange { get; init; } = string.Empty;

}