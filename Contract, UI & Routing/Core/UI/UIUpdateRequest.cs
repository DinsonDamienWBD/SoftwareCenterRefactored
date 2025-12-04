using System.Collections.Generic;

namespace SoftwareCenter.Core.UI.Requests;

/// <summary>
/// Represents a request to dynamically update a part of the UI.
/// </summary>
public class UIUpdateRequest
{
    /// <summary>
    /// Gets or sets the unique identifier of the owner making the request.
    /// </summary>
    public required string OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the target UI element (e.g., a container or card) to update.
    /// </summary>
    public required string TargetElementId { get; set; }

    /// <summary>
    /// Gets or sets the fully custom HTML/CSS/JS code to be rendered inside the target element.
    /// This will replace the existing content of the target element.
    /// </summary>
    public string? CustomHtmlContent { get; set; }
}