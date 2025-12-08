using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoftwareCenter.Core.Discovery;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// Responsible for generating the runtime capability manifest.
    /// </summary>
    public class RegistryManifestService
    {
        private readonly ModuleLoader _moduleLoader;
        private readonly Dictionary<Assembly, XmlDocumentationParser> _xmlParsers = new Dictionary<Assembly, XmlDocumentationParser>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RegistryManifestService"/> class.
        /// </summary>
        /// <param name="moduleLoader">The module loader used to discover capabilities.</param>
        public RegistryManifestService(ModuleLoader moduleLoader)
        {
            _moduleLoader = moduleLoader;
        }

        /// <summary>
        /// Generates a manifest of all currently discovered capabilities.
        /// </summary>
        /// <returns>A <see cref="RegistryManifest"/> object.</returns>
        public RegistryManifest GenerateManifest()
        {
            var capabilities = new List<CapabilityDescriptor>();

            var commandHandlers = _moduleLoader.GetDiscoveredCommandHandlers();
            foreach (var handlerInfo in commandHandlers)
            {
                var assembly = handlerInfo.CommandType.Assembly;
                if (!_xmlParsers.ContainsKey(assembly))
                {
                    XmlDocumentationParser.TryCreateForAssembly(assembly, out var parser);
                    _xmlParsers[assembly] = parser; // Cache it, even if null
                }

                var xmlParser = _xmlParsers[assembly];
                var description = xmlParser?.GetTypeSummary(handlerInfo.CommandType) ?? string.Empty;
                var status = !string.IsNullOrEmpty(description) ? CapabilityStatus.Available : CapabilityStatus.MetadataMissing;

                // Get parameters from the command's constructor
                var parameters = new List<ParameterDescriptor>();
                if (xmlParser != null)
                {
                    var constructor = handlerInfo.CommandType.GetConstructors()
                        .OrderByDescending(c => c.GetParameters().Length)
                        .FirstOrDefault(); // Get the ctor with the most parameters

                    if (constructor != null)
                    {
                        parameters.AddRange(xmlParser.GetConstructorParameters(constructor));
                    }
                }

                var descriptor = new CapabilityDescriptor(
                    name: handlerInfo.CommandType.Name,
                    description: description,
                    type: CapabilityType.Command,
                    status: status,
                    contractTypeName: handlerInfo.CommandType.FullName,
                    handlerTypeName: handlerInfo.HandlerType.FullName,
                    owningModuleId: assembly.GetName().Name,
                    parameters: parameters
                );
                capabilities.Add(descriptor);
            }

            return new RegistryManifest(capabilities);
        }
    }
}
