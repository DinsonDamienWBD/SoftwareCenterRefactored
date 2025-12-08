using Microsoft.Extensions.DependencyInjection;

namespace SoftwareCenter.Core.Modules;

/// <summary>
/// Represents the main entry point for a loadable module.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Called by the Kernel during startup to allow the module to register its services, commands, and event handlers.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    void Register(IServiceCollection services);
}