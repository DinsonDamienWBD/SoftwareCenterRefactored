namespace SoftwareCenter.Core.Discovery
{
    /// <summary>
    /// Defines the runtime status of a registered capability.
    /// </summary>
    public enum CapabilityStatus
    {
        /// <summary>
        /// The capability is available and fully supported.
        /// </summary>
        Available,

        /// <summary>
        /// The capability is functional but its "rich metadata" (e.g., from XML comments) could not be loaded.
        /// The developer should be warned to provide documentation.
        /// </summary>
        MetadataMissing,

        /// <summary>
        /// The capability is obsolete and will be removed in a future version. Use is discouraged.
        /// </summary>
        Obsolete,

        /// <summary>
        /// The capability is deprecated and has been replaced by a newer one. It is kept for backward compatibility.
        /// </summary>
        Deprecated,

        /// <summary>
        /// The capability is experimental and its API may change. Use with caution.
        /// </summary>
        Experimental
    }
}
