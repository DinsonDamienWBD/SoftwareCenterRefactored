namespace SoftwareCenter.Core.UI.Client;

/// <summary>
/// Data Transfer Object representing a content container, sent to the client for rendering.
/// </summary>
public class ClientContentContainer
{
    public required string Id { get; set; }
    public required string OwnerId { get; set; }
    public required string AssociatedNavButtonId { get; set; }

    /// <summary>
    /// If true, a 'pop-out' button will be rendered for this container.
    /// </summary>
    public bool HasSpa { get; set; }

    /// <summary>
    /// The URL for the pop-out SPA.
    /// </summary>
    public string? SpaUrl { get; set; }
}