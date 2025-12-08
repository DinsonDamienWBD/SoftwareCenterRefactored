using System.Collections.Generic;

namespace SoftwareCenter.Core.Discovery
{
    /// <summary>
    /// Represents a snapshot of all capabilities registered with the Kernel at a specific moment in time.
    /// This object is the result of the "GetRegistryManifest" query.
    /// </summary>
    public class RegistryManifest
    {
        /// <summary>
        /// Gets the list of all capabilities currently registered in the system.
        /// </summary>
        public IReadOnlyList<CapabilityDescriptor> Capabilities { get; }

        /// <summary>
        /// Gets the timestamp of when this manifest was generated.
        /// </summary>
        public System.DateTimeOffset GeneratedAt { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryManifest"/> class.
        /// </summary>
        /// <param name="capabilities">The list of all registered capabilities.</param>
        public RegistryManifest(IReadOnlyList<CapabilityDescriptor> capabilities)
        {
            Capabilities = capabilities;
            GeneratedAt = System.DateTimeOffset.UtcNow;
        }
    }
}
