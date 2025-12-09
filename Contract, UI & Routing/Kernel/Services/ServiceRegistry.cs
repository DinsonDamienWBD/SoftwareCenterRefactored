using System;
using System.Collections.Concurrent;
using System.Linq;

namespace SoftwareCenter.Kernel.Services
{
    public class ServiceRegistry : IServiceRegistry
    {
        private class ServiceRegistration
        {
            public Type ServiceType { get; }
            public Type ImplementationType { get; }
            public string OwningModuleId { get; }

            public ServiceRegistration(Type serviceType, Type implementationType, string owningModuleId)
            {
                ServiceType = serviceType;
                ImplementationType = implementationType;
                OwningModuleId = owningModuleId;
            }
        }

        private readonly ConcurrentDictionary<Type, ServiceRegistration> _serviceRegistrations = new ConcurrentDictionary<Type, ServiceRegistration>();

        public void Register<TService, TImplementation>(string owningModuleId)
        {
            var registration = new ServiceRegistration(typeof(TService), typeof(TImplementation), owningModuleId);
            _serviceRegistrations.AddOrUpdate(
                typeof(TService), 
                registration, 
                (key, oldValue) => registration);
        }

        public Type Get<TService>()
        {
            if (_serviceRegistrations.TryGetValue(typeof(TService), out var registration))
            {
                return registration.ImplementationType;
            }
            return null;
        }

        public void UnregisterModuleServices(string moduleId)
        {
            var servicesToUnregister = _serviceRegistrations.Values.Where(r => r.OwningModuleId == moduleId).ToList();
            foreach (var registration in servicesToUnregister)
            {
                _serviceRegistrations.TryRemove(registration.ServiceType, out _);
            }
        }
    }
}