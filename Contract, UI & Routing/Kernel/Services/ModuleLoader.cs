using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SoftwareCenter.Core.Commands;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// Discovers capabilities from loaded assemblies.
    /// In a real system, this would load DLLs from a modules directory.
    /// For now, it scans all currently loaded AppDomain assemblies.
    /// </summary>
    public class ModuleLoader
    {
        public class DiscoveredHandler
        {
            public Type HandlerType { get; set; }
            public Type CommandType { get; set; }
            public Type InterfaceType { get; set; }
        }

        public List<DiscoveredHandler> GetDiscoveredCommandHandlers()
        {
            var handlers = new List<DiscoveredHandler>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var handlerInterface = typeof(ICommandHandler<,>);

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (type.IsAbstract || type.IsInterface) continue;

                        var interfaces = type.GetInterfaces()
                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                            .ToList();

                        foreach (var i in interfaces)
                        {
                            var commandType = i.GetGenericArguments()[0];
                            handlers.Add(new DiscoveredHandler
                            {
                                HandlerType = type,
                                CommandType = commandType,
                                InterfaceType = i
                            });
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // Ignore assemblies that fail to load types
                }
            }
            return handlers;
        }
    }
}
