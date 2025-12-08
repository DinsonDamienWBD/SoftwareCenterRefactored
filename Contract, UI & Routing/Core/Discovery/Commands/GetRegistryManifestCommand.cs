using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Discovery;

namespace SoftwareCenter.Core.Discovery.Commands
{
    /// <summary>
    /// A command to request the Kernel's complete runtime registry manifest.
    /// When executed, this command returns a <see cref="RegistryManifest"/> object.
    /// </summary>
    public class GetRegistryManifestCommand : ICommand<RegistryManifest>
    {
        // This command has no parameters as it's a simple query for the entire state.
    }
}
