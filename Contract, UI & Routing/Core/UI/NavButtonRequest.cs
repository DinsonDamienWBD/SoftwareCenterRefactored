namespace SoftwareCenter.Core.UI.Requests;

/// <summary>
/// Represents a request to create a navigation button in the main navigation area.
/// </summary>
public class NavButtonRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the owner making the request (e.g., a module ID).
    /// </summary>
    public required string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the text label to be displayed on the button.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Gets or sets an optional icon to be displayed on the button (e.g., a CSS class for a font icon).
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Gets or sets the priority for ordering the button in the navigation area. Lower numbers appear first.
    /// </summary>
    public int Priority { get; set; } = 100;
}