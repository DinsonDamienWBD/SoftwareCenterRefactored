namespace SoftwareCenter.Core.Diagnostics;

/// <summary>
/// Represents a single step in a command's execution pipeline.
/// </summary>
public class TraceHop
{
    /// <summary>
    /// The name of the component that processed this step (e.g., "Kernel.Router", "Module.AppManager").
    /// </summary>
    public string Component { get; }

    /// <summary>
    /// A message describing the action taken at this hop.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// The timestamp when this hop was recorded.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public TraceHop(string component, string message)
    {
        Component = component;
        Message = message;
    }
}