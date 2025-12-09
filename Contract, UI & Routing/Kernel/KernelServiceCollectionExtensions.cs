using Microsoft.Extensions.DependencyInjection;
using SoftwareCenter.Kernel.Handlers;
using SoftwareCenter.Core.Discovery.Commands;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Discovery;
using SoftwareCenter.Kernel.Services;
using SoftwareCenter.Core.Events;
using SoftwareCenter.Core.Errors;
using SoftwareCenter.Core.Modules; // Added for IModule
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting; // Added for IHostedService and AddHostedService

namespace SoftwareCenter.Kernel
{
    public static class KernelServiceCollectionExtensions
    {
        public static IServiceCollection AddKernel(this IServiceCollection services)
        {
            // Register core Kernel services that don't depend on module discovery
            services.AddLogging();
            services.AddSingleton<IErrorHandler, DefaultErrorHandler>();
            services.AddSingleton<IServiceRegistry, ServiceRegistry>();
            services.AddSingleton<IServiceRoutingRegistry, ServiceRoutingRegistry>();
            services.AddSingleton<ModuleLoader>(); // Depends on the above

            services.AddSingleton<ISmartCommandRouter, SmartCommandRouter>();
            services.AddSingleton<ICommandBus, CommandBus>();
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<CommandFactory>();
            services.AddSingleton<RegistryManifestService>();
            services.AddSingleton<GlobalDataStore>();
            services.AddSingleton<JobSchedulerService>();
            services.AddHostedService<JobSchedulerService>(sp => sp.GetRequiredService<JobSchedulerService>());

            // A temporary service provider is created to resolve the ModuleLoader and other services needed for discovery
            var tempServiceProvider = services.BuildServiceProvider();
            var moduleLoader = tempServiceProvider.GetRequiredService<ModuleLoader>();
            var serviceRoutingRegistry = tempServiceProvider.GetRequiredService<IServiceRoutingRegistry>();

            // Load modules and discover their capabilities
            moduleLoader.LoadModulesFromDisk();
            
            // Register discovered modules and their services
            foreach (var moduleInfo in moduleLoader.GetLoadedModules())
            {
                if(moduleInfo.Instance != null)
                {
                    services.AddSingleton(moduleInfo.Instance.GetType(), moduleInfo.Instance);
                    services.AddSingleton(typeof(IModule), moduleInfo.Instance);
                    moduleInfo.Instance.ConfigureServices(services);
                }
            }

            // Register discovered handlers
            foreach (var handler in moduleLoader.GetDiscoveredHandlers())
            {
                services.AddTransient(handler.HandlerType);
                serviceRoutingRegistry.RegisterHandler(handler.ContractType, handler.HandlerType, handler.InterfaceType, handler.Priority, handler.OwningModuleId);
            }

            // Register default Kernel handlers
            services.AddTransient<DefaultLogCommandHandler>();
            serviceRoutingRegistry.RegisterHandler(typeof(LogCommand), typeof(DefaultLogCommandHandler), typeof(ICommandHandler<LogCommand>), -100, "Kernel");
            services.AddTransient<GetRegistryManifestCommandHandler>();
            serviceRoutingRegistry.RegisterHandler(typeof(GetRegistryManifestCommand), typeof(GetRegistryManifestCommandHandler), typeof(ICommandHandler<GetRegistryManifestCommand, RegistryManifest>), 0, "Kernel");

            // Dynamically register all discovered command validators
            var assembliesToScan = moduleLoader.GetLoadedAssemblies();
            var commandValidatorInterface = typeof(ICommandValidator<>);
            foreach (var assembly in assembliesToScan)
            {
                var validatorTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsInterface && t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandValidatorInterface));

                foreach (var validatorType in validatorTypes)
                {
                    var genericValidatorInterface = validatorType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == commandValidatorInterface);
                    services.AddTransient(genericValidatorInterface, validatorType);
                }
            }

            return services;
        }

        /// <summary>
        /// Performs post-service provider build initialization for all loaded modules.
        /// </summary>
        /// <param name="serviceProvider">The application's service provider.</param>
        /// <returns>A Task representing the asynchronous initialization operation.</returns>
        public static async Task UseKernel(this IServiceProvider serviceProvider)
        {
            var modules = serviceProvider.GetServices<IModule>();
            foreach (var module in modules)
            {
                await module.Initialize(serviceProvider);
            }
        }
    }
}
