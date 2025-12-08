using Microsoft.Extensions.DependencyInjection;
using SoftwareCenter.Kernel.Handlers;
using SoftwareCenter.Core.Discovery.Commands;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Discovery;
using SoftwareCenter.Kernel.Services;

namespace SoftwareCenter.Kernel
{
    public static class KernelServiceCollectionExtensions
    {
        public static IServiceCollection AddKernel(this IServiceCollection services)
        {
            // Register Kernel services
            services.AddSingleton<ModuleLoader>();
            services.AddSingleton<RegistryManifestService>();
            services.AddSingleton<ICommandBus, CommandBus>();
            services.AddSingleton<GlobalDataStore>();

            // Register Handlers
            services.AddTransient<ICommandHandler<GetRegistryManifestCommand, RegistryManifest>, GetRegistryManifestCommandHandler>();

            return services;
        }
    }
}
