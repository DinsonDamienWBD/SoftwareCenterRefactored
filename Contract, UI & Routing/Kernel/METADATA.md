# Project: SoftwareCenter.Kernel

## Purpose
`SoftwareCenter.Kernel` is the brain of the application. It acts as the central service broker, communication bus, and module manager. It is responsible for loading all components, wiring them together at runtime, and routing all internal communication.

## Key Responsibilities
- **Module Loading:** Discover, load, and initialize all `IModule` implementations from assemblies in the `Modules` directory.
- **Service Registry:** Maintain a runtime registry of all available services, their contracts, and their implementations.
- **Command & Event Bus:** Dispatch `ICommand` and `IEvent` objects to their registered handlers.
- **API Endpoint Registry:** Maintain a runtime registry of all discoverable API endpoints for routing and help generation.
- **Developer Help:** Provide a mechanism (e.g., a special command or API endpoint) for developers to query the registries to see what capabilities are currently available at runtime.
- **Pipeline Tracing:** Create and manage the `ITraceContext` for all operations, ensuring end-to-end traceability.

## Architectural Principles
- **Central Hub:** All inter-module and host-module communication flows through the Kernel.
- **No Business Logic:** The Kernel itself does not implement business logic (e.g., it doesn't know how to install an app). It only routes requests to the components that do.
- **Dependency Injection:** It heavily utilizes `Microsoft.Extensions.DependencyInjection` to manage dependencies and service lifetimes.
- **Referenced by Host:** The Host application initializes and holds a reference to the Kernel.

## Project Structure
```
SoftwareCenter.Kernel/
├── Services/
│   ├── CommandBus.cs
│   ├── EventBus.cs
│   ├── ModuleLoader.cs
│   └── ServiceRegistry.cs
├── KernelServiceCollectionExtensions.cs
├── SoftwareCenter.Kernel.csproj
└── METADATA.md
```

## Dependencies
- `SoftwareCenter.Core`