using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using SoftwareCenter.Core.Attributes;
using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Events;
using SoftwareCenter.Core.Jobs;
using SoftwareCenter.Core.Modules;
using SoftwareCenter.Core.Routing;
using SoftwareCenter.Core.Errors;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Kernel.Models;

namespace SoftwareCenter.Kernel.Services
{
    public class ModuleLoader
    {
        private readonly Dictionary<string, ModuleInfo> _loadedModules = new Dictionary<string, ModuleInfo>();
        private bool _initialModulesLoaded = false;
        private readonly IErrorHandler _errorHandler;
        private readonly IServiceRoutingRegistry _serviceRoutingRegistry;
        private readonly IServiceRegistry _serviceRegistry;

        public ModuleLoader(IErrorHandler errorHandler, IServiceRoutingRegistry serviceRoutingRegistry, IServiceRegistry serviceRegistry)
        {
            _errorHandler = errorHandler ?? throw new ArgumentNullException(nameof(errorHandler));
            _serviceRoutingRegistry = serviceRoutingRegistry ?? throw new ArgumentNullException(nameof(serviceRoutingRegistry));
            _serviceRegistry = serviceRegistry ?? throw new ArgumentNullException(nameof(serviceRegistry));
        }

        public class DiscoveredHandler
        {
            public Type HandlerType { get; set; }
            public Type ContractType { get; set; }
            public Type InterfaceType { get; set; }
            public int Priority { get; set; }
            public string OwningModuleId { get; set; }
        }

        public void LoadModulesFromDisk()
        {
            if (_initialModulesLoaded) return;

            var hostAssembly = Assembly.GetEntryAssembly();
            var rootPath = Path.GetDirectoryName(hostAssembly.Location);
            var modulesPath = Path.Combine(rootPath, "Modules");

            if (Directory.Exists(modulesPath))
            {
                var moduleDirectories = Directory.GetDirectories(modulesPath);
                foreach (var dir in moduleDirectories)
                {
                    var dirName = new DirectoryInfo(dir).Name;
                    var dllPath = Path.Combine(dir, $"{dirName}.dll");
                    if (File.Exists(dllPath))
                    {
                        LoadModule(dllPath);
                    }
                }
            }
            _initialModulesLoaded = true;
        }

        public Assembly LoadModule(string dllPath)
        {
            if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
            {
                _errorHandler.HandleError(new FileNotFoundException($"Module DLL not found at {dllPath}"), new TraceContext(), $"Attempted to load a module from an invalid path: {dllPath}.");
                return null;
            }

            var assemblyName = AssemblyName.GetAssemblyName(dllPath);
            var moduleId = assemblyName.Name;

            if (_loadedModules.ContainsKey(moduleId))
            {
                _errorHandler.HandleError(null, new TraceContext(), $"Module '{moduleId}' is already loaded.", isCritical: false);
                return _loadedModules[moduleId].Assembly;
            }

            try
            {
                var loadContext = new ModuleLoadContext(dllPath);
                var assembly = loadContext.LoadFromAssemblyName(assemblyName);
                
                var moduleInfo = new ModuleInfo(moduleId, assembly, loadContext);
                _loadedModules.Add(moduleId, moduleInfo);

                DiscoverModuleCapabilities(moduleInfo);

                _errorHandler.HandleError(null, new TraceContext(), $"Module '{moduleId}' loaded successfully.", isCritical: false);
                return assembly;
            }
            catch (Exception ex)
            {
                _errorHandler.HandleError(ex, new TraceContext(), $"Error loading module from {dllPath}.");
                return null;
            }
        }
        
        public void UnloadModule(string moduleId)
        {
            if (!_loadedModules.TryGetValue(moduleId, out var moduleInfo))
            {
                _errorHandler.HandleError(null, new TraceContext(), $"Module '{moduleId}' not found for unloading.", isCritical: false);
                return;
            }

            try
            {
                moduleInfo.State = ModuleState.Unloading;

                _serviceRoutingRegistry.UnregisterModuleHandlers(moduleId);
                _serviceRegistry.UnregisterModuleServices(moduleId);

                _loadedModules.Remove(moduleId);
                moduleInfo.LoadContext.Unload();
                moduleInfo.State = ModuleState.Unloaded;

                _errorHandler.HandleError(null, new TraceContext(), $"Module '{moduleId}' unloaded successfully.", isCritical: false);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                moduleInfo.State = ModuleState.Error;
                _errorHandler.HandleError(ex, new TraceContext(), $"Error unloading module '{moduleId}'.");
            }
        }

        private void DiscoverModuleCapabilities(ModuleInfo moduleInfo)
        {
            var assembly = moduleInfo.Assembly;
            var moduleId = moduleInfo.ModuleId;

            try
            {
                var types = assembly.GetTypes();
                
                var apiEndpointType = typeof(IApiEndpoint);
                var moduleType = typeof(IModule);

                var commandHandlerInterface = typeof(ICommandHandler<,>);
                var fireAndForgetCommandHandlerInterface = typeof(ICommandHandler<>);
                var eventHandlerInterface = typeof(IEventHandler<>);
                var jobHandlerInterface = typeof(IJobHandler<>);


                foreach (var type in types.Where(t => !t.IsAbstract && !t.IsInterface))
                {
                    // Discover Handlers
                    var handlerPriority = type.GetCustomAttribute<HandlerPriorityAttribute>()?.Priority ?? 0;
                    var interfaces = type.GetInterfaces();
                    bool isHandler = false;
                    foreach (var i in interfaces)
                    {
                        if (i.IsGenericType)
                        {
                            var genericDef = i.GetGenericTypeDefinition();
                            if (genericDef == commandHandlerInterface || genericDef == fireAndForgetCommandHandlerInterface || genericDef == eventHandlerInterface || genericDef == jobHandlerInterface)
                            {
                                moduleInfo.Handlers.Add(new DiscoveredHandler { HandlerType = type, ContractType = i.GetGenericArguments()[0], InterfaceType = i, Priority = handlerPriority, OwningModuleId = moduleId });
                                isHandler = true;
                            }
                        }
                    }

                    // Discover API Endpoints
                    if (apiEndpointType.IsAssignableFrom(type))
                    {
                        moduleInfo.ApiEndpoints.Add(type);
                    }
                    // Discover Services (any class that implements an interface and is not a handler or endpoint)
                    else if (interfaces.Any() && !isHandler)
                    {
                        moduleInfo.Services.Add(type);
                    }


                    // Discover IModule implementation
                    if (moduleType.IsAssignableFrom(type))
                    {
                        moduleInfo.Instance = (IModule)Activator.CreateInstance(type);
                    }
                }
            }
            catch (Exception ex)
            {
                moduleInfo.State = ModuleState.Error;
                _errorHandler.HandleError(ex, new TraceContext(), $"Error discovering capabilities in module '{moduleId}'.");
            }
        }

        public List<Assembly> GetLoadedAssemblies()
        {
            LoadModulesFromDisk();
            var assemblies = new List<Assembly> { Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly() };
            assemblies.AddRange(_loadedModules.Values.Select(m => m.Assembly));
            return assemblies.Distinct().ToList();
        }

        public IEnumerable<ModuleInfo> GetLoadedModules() => _loadedModules.Values;

        public List<DiscoveredHandler> GetDiscoveredHandlers()
        {
            LoadModulesFromDisk();
            return _loadedModules.Values.SelectMany(m => m.Handlers).ToList();
        }

        public List<IApiEndpoint> GetDiscoveredApiEndpoints()
        {
            LoadModulesFromDisk();
            var endpoints = new List<IApiEndpoint>();
            foreach(var module in _loadedModules.Values)
            {
                foreach(var endpointType in module.ApiEndpoints)
                {
                    try
                    {
                        endpoints.Add((IApiEndpoint)Activator.CreateInstance(endpointType));
                    }
                    catch (Exception ex)
                    {
                        _errorHandler.HandleError(ex, new TraceContext(), $"Error instantiating IApiEndpoint '{endpointType.Name}'.");
                    }
                }
            }
            return endpoints;
        }

        public List<Type> GetDiscoveredModuleTypes()
        {
            LoadModulesFromDisk();
            return _loadedModules.Values.Where(m => m.Instance != null).Select(m => m.Instance.GetType()).ToList();
        }
    }
}

