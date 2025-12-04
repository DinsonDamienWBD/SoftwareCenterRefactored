namespace SoftwareCenter.Core.UI.Client;

/// <summary>
/// Data Transfer Object representing a navigation button, sent to the client for rendering.
/// </summary>
public class ClientNavButton
{
    public required string Id { get; set; }
    public required string Label { get; set; }
    public string? Icon { get; set; }
    public int Priority { get; set; }
}