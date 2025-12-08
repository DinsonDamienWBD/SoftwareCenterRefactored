namespace SoftwareCenter.Core.Diagnostics;

/// <summary>
/// Defines the contract for carrying diagnostic and tracing information
/// through the execution pipeline of a command or request.
/// </summary>
public interface ITraceContext
{
    /// <summary>
    /// A unique identifier for the entire operation, from start to finish.
    /// </summary>
    Guid TraceId { get; }

    /// <summary>
    /// An ordered list of hops, representing the journey of the operation through the system.
    /// </summary>
    List<TraceHop> History { get; }

    /// <summary>
    /// Adds a new hop to the trace history.
    /// </summary>
    void AddHop(string component, string message);
}