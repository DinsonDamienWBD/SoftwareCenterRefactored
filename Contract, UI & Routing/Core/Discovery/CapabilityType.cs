namespace SoftwareCenter.Core.Discovery
{
    /// <summary>
    /// Defines the type of a capability registered with the Kernel.
    /// </summary>
    public enum CapabilityType
    {
        /// <summary>
        /// A command that can be executed.
        /// </summary>
        Command,

        /// <summary>
        /// An event that can be published.
        /// </summary>
        Event,

        /// <summary>
        /// A background job that runs on a schedule.
        /// </summary>
        Job,

        /// <summary>
        /// An API endpoint that can be called.
        /// </summary>
        ApiEndpoint,

        /// <summary>
        /// A general-purpose service.
        /// </summary>
        Service
    }
}
