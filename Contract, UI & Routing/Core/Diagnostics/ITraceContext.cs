using System;
using System.Collections.Generic;

namespace SoftwareCenter.Core.Diagnostics
{
    /// <summary>
    /// Provides a context for a single, end-to-end operation, allowing for correlation
    /// of logs and diagnostics across different services and modules.
    /// </summary>
    public interface ITraceContext
    {
        /// <summary>
        /// Gets the unique identifier for this specific trace instance.
        /// </summary>
        Guid TraceId { get; }

        /// <summary>
        /// Gets a key-value store for adding contextual information to the trace.
        /// This data can be used by advanced logging modules to enrich logs.
        /// </summary>
        IDictionary<string, object> Items { get; }
    }

    /// <summary>
    /// Default implementation of <see cref="ITraceContext"/>.
    /// </summary>
    public class TraceContext : ITraceContext
    {
        /// <inheritdoc />
        public Guid TraceId { get; }

        /// <inheritdoc />
        public IDictionary<string, object> Items { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceContext"/> class with a new TraceId.
        /// </summary>
        public TraceContext()
        {
            TraceId = Guid.NewGuid();
            Items = new Dictionary<string, object>();
        }
    }
}
