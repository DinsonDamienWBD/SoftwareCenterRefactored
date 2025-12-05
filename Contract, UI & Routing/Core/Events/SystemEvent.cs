using SoftwareCenter.Core.Diagnostics;
using System;
using System.Collections.Generic;

namespace SoftwareCenter.Core.Events;

/// <summary>
/// A concrete, general-purpose implementation of the <see cref="IEvent"/> interface.
/// </summary>
public class SystemEvent : IEvent
{
    /// <inheritdoc />
    public string Name { get; }

    /// <inheritdoc />
    public Dictionary<string, object> Data { get; }

    /// <inheritdoc />
    public DateTime Timestamp { get; }

    /// <inheritdoc />
    public Guid? TraceId { get; }

    /// <inheritdoc />
    public string SourceId { get; set; } // Settable by the kernel/bus

    public SystemEvent(string name, Dictionary<string, object>? data = null)
    {
        Name = name;
        Data = data ?? new Dictionary<string, object>();
        Timestamp = DateTime.UtcNow;
        TraceId = TraceContext.CurrentTraceId;
        SourceId = "Unknown"; // Default value, to be overwritten by the publisher if needed
    }
}