﻿using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Data;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Events;
using SoftwareCenter.Core.Jobs;
using SoftwareCenter.Core.Kernel;
using SoftwareCenter.Core.Routing;
using SoftwareCenter.Kernel.Engine;
using SoftwareCenter.Kernel.Routing;
using SoftwareCenter.Kernel.Services;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace SoftwareCenter.Kernel
{
    /// <summary>
    /// The concrete implementation of the IKernel interface, acting as the "brain" of the application.
    /// It serves as the Composition Root, bootstrapping all core services, managing module lifecycles,
    /// and providing a central point for service location.
    /// The primary concrete implementation of the IKernel interface. 
    /// It aggregates all the internal services (router, event bus, etc.) and exposes them to modules.
    /// It also provides a simple service locator pattern for shared services.
    /// </summary>
    public class StandardKernel : IKernel
    {
        /// <inheritdoc />
        public IRouter Router { get; }
        /// <inheritdoc />
        public IGlobalDataStore DataStore { get; }
        /// <inheritdoc />
        public IEventBus EventBus { get; }
        /// <inheritdoc />
        public IJobScheduler JobScheduler { get; }

        // Internals & DI Container
        private readonly HandlerRegistry _registry;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<StandardKernel> _logger;
        private readonly ModuleLoader _loader;
        private readonly Dictionary<Type, object> _services = new();
        private readonly KernelLogger _kernelLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardKernel"/> class.
        /// This constructs the entire application brain, bootstrapping all core services
        /// like routing, data storage, eventing, and module loading.
        /// </summary>
        public StandardKernel(ILoggerFactory loggerFactory, string logDirectory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<StandardKernel>();

            // A. Bootstrap Core Services
            _registry = new HandlerRegistry();
            DataStore = new GlobalDataStore();
            EventBus = new DefaultEventBus(_loggerFactory.CreateLogger<DefaultEventBus>());
            _kernelLogger = new KernelLogger();

            // B. Bootstrap Intelligence
            Router = new SmartRouter(_registry, EventBus);
            JobScheduler = new JobScheduler(_loggerFactory.CreateLogger<JobScheduler>());

            // C. Bootstrap Engine
            _loader = new ModuleLoader(this, _registry);
            
            // D. Setup Default Logger
            var hostLogger = new HostLogger(logDirectory);
            _kernelLogger.RegisterLogger(hostLogger);

            // E. Register Services for DI / Service Location
            RegisterService<ILoggerFactory>(_loggerFactory);
            RegisterService<IScLogger>(_kernelLogger); // Register the central logger
            RegisterService<IGlobalDataStore>(DataStore);
            RegisterService<IEventBus>(EventBus);
            RegisterService<IJobScheduler>(JobScheduler);
            RegisterService<IRouter>(Router);
            // Register the kernel itself as a service so modules can access it.
            RegisterService<IKernel>(this);

            // F. Register Internal System Commands (The Missing Link)
            RegisterSystemCommands();
        }

        private void RegisterSystemCommands()
        {
            // 1. System.Log.Config: Allows Runtime toggling of Verbose Logging
            _registry.Register(
                "System.Log.Config",
                async (cmd) =>
                {
                    if (cmd.Parameters.TryGetValue("Verbose", out var v) && v is bool verbose)
                    {
                        // Persist the setting
                        await DataStore.StoreAsync("Logging.Verbose", verbose, DataPolicy.Persistent);
                        // An advanced logger module would subscribe to this event to reconfigure itself.
                        // The Kernel no longer needs to know about logger-specific settings.
                        await EventBus.PublishAsync(new SystemEvent("Logging.ConfigurationChanged"));
                        return Result.FromSuccess(true, $"Verbose Logging set to {verbose}");
                    }
                    return Result.FromFailure("Invalid Parameters. Expected 'Verbose' (bool).");
                },
                new RouteMetadata
                {
                    CommandId = "System.Log.Config",
                    Description = "Toggles Verbose Logging. Params: { 'Verbose': bool }",
                    SourceModule = "Kernel",
                    Version = "1.0.0"
                },
                100 // High Priority
            );

            // 2. System.Help: The Discovery API
            _registry.Register(
                "System.Help",
                (cmd) =>
                {
                    var manifest = _registry.GetRegistryManifest();
                    return Task.FromResult<IResult>(Result.FromSuccess(manifest));
                },
                new RouteMetadata
                {
                    CommandId = "System.Help",
                    Description = "Returns a manifest of all registered commands.",
                    SourceModule = "Kernel",
                    Version = "1.0.0"
                },
                100
            );
        }

        // --- IKernel Implementation ---

        /// <inheritdoc />
        public void Register(string commandName, Func<ICommand, Task<IResult>> handler, RouteMetadata metadata)
        {
            // Default priority 0. Modules can cast to specific Registry types if they need advanced priority.
            _registry.Register(commandName, handler, metadata, 0);
        }

        /// <inheritdoc />
        public Task<IResult> RouteAsync(ICommand command)
        {
            return Router.RouteAsync(command);
        }

        /// <inheritdoc />
        public T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }
            // Returning null is generally safer for loose coupling than throwing an exception.
            return null;
        }

        /// <inheritdoc />
        public void RegisterService<T>(T service) where T : class
        {
            _services[typeof(T)] = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Asynchronously starts the kernel and loads all discovered modules.
        /// </summary>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public async Task StartAsync()
        {
            // Load standard modules from ./Modules
            var modulesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules");

            // Ensure directory exists to prevent crashes on fresh installs
            Directory.CreateDirectory(modulesPath);

            // Log start
            _logger.LogInformation("Kernel starting...");

            // Load
            await _loader.LoadModulesAsync(modulesPath);

            _logger.LogInformation("Kernel ready. All modules loaded.");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            (DataStore as IDisposable)?.Dispose();
            (JobScheduler as IDisposable)?.Dispose();
        }

        // Helper stub for internal Kernel logs
        private class JobCommandStub : ICommand
        {
            public string Name { get; }
            public Dictionary<string, object> Parameters { get; } = new(); public Guid TraceId { get; } = Guid.NewGuid(); public List<TraceHop> History { get; } = new();
            public JobCommandStub(string name) { Name = name; }
        }
    }
}