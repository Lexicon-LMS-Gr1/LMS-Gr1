namespace LMS.Blazor.Client.Components.Shared
{
    public static class BadgeHelper
    {
        public static string GetBadgeColor(string type)
        {
            return type switch
            {
                "Lecture" => "bg-primary",
                "Workshop" => "bg-warning text-dark",
                "Assignment" => "bg-info text-dark",
                "Exam" => "bg-danger",
                "Seminar" => "bg-success",
                _ => "bg-secondary"
            };
        }
    }
}
