using System;
using System.Collections.Generic;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// A simple in-memory implementation of IServiceRegistry.
    /// NOTE: This is a placeholder and not thread-safe.
    /// </summary>
    public class ServiceRegistry //: IServiceRegistry
    {
        private readonly Dictionary<Type, Type> _serviceImplementations = new Dictionary<Type, Type>();

        public void Register<TService, TImplementation>()
        {
            _serviceImplementations[typeof(TService)] = typeof(TImplementation);
        }

        public Type Get<TService>()
        {
            return _serviceImplementations.GetValueOrDefault(typeof(TService));
        }
    }
}
