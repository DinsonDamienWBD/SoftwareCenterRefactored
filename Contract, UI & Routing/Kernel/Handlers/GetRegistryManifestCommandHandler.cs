using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Discovery;
using SoftwareCenter.Core.Discovery.Commands;
using SoftwareCenter.Kernel.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftwareCenter.Kernel.Handlers
{
    /// <summary>
    /// Handles the <see cref="GetRegistryManifestCommand"/> to provide a runtime manifest of all capabilities.
    /// </summary>
    public class GetRegistryManifestCommandHandler : ICommandHandler<GetRegistryManifestCommand, RegistryManifest>
    {
        private readonly RegistryManifestService _manifestService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetRegistryManifestCommandHandler"/> class.
        /// </summary>
        /// <param name="manifestService">The service used to generate the manifest.</param>
        public GetRegistryManifestCommandHandler(RegistryManifestService manifestService)
        {
            _manifestService = manifestService;
        }

        /// <summary>
        /// Handles the command asynchronously.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="traceContext">The context for tracing the operation.</param>
        /// <returns>A <see cref="RegistryManifest"/> containing all registered capabilities.</returns>
        public Task<RegistryManifest> Handle(GetRegistryManifestCommand command, ITraceContext traceContext)
        {
            // We could add information to the trace context here if needed, e.g.:
            // traceContext.Items["ManifestGenerator.Version"] = "1.0";

            var manifest = _manifestService.GenerateManifest();
            return Task.FromResult(manifest);
        }
    }
}
