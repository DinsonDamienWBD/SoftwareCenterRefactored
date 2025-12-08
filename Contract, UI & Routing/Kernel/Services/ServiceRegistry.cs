using System;
using System.Collections.Concurrent;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// A thread-safe in-memory implementation of IServiceRegistry.
    /// </summary>
    public class ServiceRegistry
    {
        private readonly ConcurrentDictionary<Type, Type> _serviceImplementations = new ConcurrentDictionary<Type, Type>();

        public void Register<TService, TImplementation>()
        {
            _serviceImplementations.AddOrUpdate(
                typeof(TService), 
                typeof(TImplementation), 
                (key, oldValue) => typeof(TImplementation));
        }

        public Type Get<TService>()
        {
            if (_serviceImplementations.TryGetValue(typeof(TService), out var implType))
            {
                return implType;
            }
            return null;
        }
    }
}