using Microsoft.AspNetCore.Components;

namespace WebUi.Components.Shared;

/// <summary>
/// Liquid-glass loading overlay. Wrap content inside this component;
/// when <see cref="Visible"/> is true, a frosted-glass spinner covers the content area.
/// </summary>
public partial class LoadingOverlay
{
    /// <summary>Show or hide the overlay.</summary>
    [Parameter] public bool Visible { get; set; }

    /// <summary>Optional contextual message displayed under the spinner.</summary>
    [Parameter] public string? Text { get; set; }

    /// <summary>Content to overlay on top of when visible.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }
}
