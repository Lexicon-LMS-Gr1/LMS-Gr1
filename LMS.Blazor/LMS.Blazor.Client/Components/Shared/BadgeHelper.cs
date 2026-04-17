namespace LMS.Blazor.Client.Components.Shared
{
    public static class BadgeHelper
    {
        public static string GetBadgeColor(string activityTypeName)
        {
            return activityTypeName switch
            {
                "Föreläsning" => "bg-primary",
                "Inlämning" => "bg-info text-dark",
                "Workshop" => "bg-warning text-dark",
                "Examination" => "bg-danger",
                //"Seminar" => "bg-success",
                _ => "bg-secondary"
            };
        }
    }
}
