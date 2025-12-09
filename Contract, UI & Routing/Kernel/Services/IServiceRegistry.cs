using System;

namespace SoftwareCenter.Kernel.Services
{
    public interface IServiceRegistry
    {
        void Register<TService, TImplementation>(string owningModuleId);
        Type Get<TService>();
        void UnregisterModuleServices(string moduleId);
    }
}
