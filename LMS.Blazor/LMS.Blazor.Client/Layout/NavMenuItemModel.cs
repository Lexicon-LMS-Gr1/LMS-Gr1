using Microsoft.AspNetCore.Components.Routing;

namespace LMS.Blazor.Client.Layout
{
    public record NavMenuItemModel(
    string Icon,
    string Text,
    string Href,
    NavLinkMatch Match = NavLinkMatch.Prefix);
}