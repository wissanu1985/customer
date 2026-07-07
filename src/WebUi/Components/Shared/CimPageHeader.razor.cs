using Microsoft.AspNetCore.Components;

namespace WebUi.Components.Shared;

public partial class CimPageHeader
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Description { get; set; }
    [Parameter] public List<CimCrumb>? Crumbs { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }
}

public sealed class CimCrumb
{
    public string Label { get; init; } = string.Empty;
    public string? Href { get; init; }
}
