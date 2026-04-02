
namespace LMS.Blazor.Client.Models
{

    // Behövs eftersom vi inte har EditContext/manuell validering på modulnivå än, så att vi kan visa modulspecifika felmeddelanden.
    public class ModuleValidationErrors
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? General { get; set; }

        public bool HasAnyError =>
                  !string.IsNullOrWhiteSpace(Name) ||
                  !string.IsNullOrWhiteSpace(Description) ||
                  !string.IsNullOrWhiteSpace(StartDate) ||
                  !string.IsNullOrWhiteSpace(EndDate) ||
                  !string.IsNullOrWhiteSpace(General);
    }
}