using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SoftwareCenter.Core.Modules
{
    /// <summary>
    /// The primary contract for a loadable module.
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Gets the unique, machine-readable ID of the module (e.g., "SoftwareCenter.AppManager").
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the human-readable name of the module (e.g., "Application Manager").
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Allows the module to register its services (command handlers, etc.) with the application's
        /// dependency injection container. This method is called during service collection build time.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Allows the module to perform any post-service configuration setup, such as subscribing to events,
        /// starting background services, or performing initial data loading. This method is called after
        /// the service provider has been built.
        /// </summary>
        /// <param name="serviceProvider">The application's service provider.</param>
        /// <returns>A Task representing the asynchronous initialization operation.</returns>
        Task Initialize(IServiceProvider serviceProvider);
    }
}
