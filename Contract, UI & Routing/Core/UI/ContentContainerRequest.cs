namespace SoftwareCenter.Core.UI.Requests;

/// <summary>
/// Represents a request to create a content container in the UI.
/// </summary>
public class ContentContainerRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the owner making the request (e.g., a module ID).
    /// </summary>
    public required string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the navigation button this container is associated with.
    /// When the nav button is clicked, this container will be shown.
    /// </summary>
    public required string AssociatedNavButtonId { get; set; }
    public string? SpaUrl { get; set; }
}