namespace SoftwareCenter.UI.Engine.Services;

public enum UIElementType { NavButton, Container, Card }

/// <summary>
/// Internal record for tracking the state and ownership of a UI element.
/// </summary>
public record UIElement
{
    public string Id { get; }
    public string OwnerId { get; }
    public UIElementType Type { get; }
    public object? StateObject { get; set; } // Holds the ClientNavButton or ClientContentContainer

    public UIElement(string id, string ownerId, UIElementType type, object? stateObject = null)
    {
        Id = id;
        OwnerId = ownerId;
        Type = type;
        StateObject = stateObject;
    }
}