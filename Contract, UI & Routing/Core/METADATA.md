# Project: SoftwareCenter.Core

## Purpose
`SoftwareCenter.Core` is the foundational contract library for the entire Software Center ecosystem. It defines the essential, non-negotiable interfaces, abstract classes, and data models that enable the decoupled, modular architecture. It is designed to be minimal, lightweight, and extremely stable.

## Key Responsibilities
- Define the `IModule` interface, the primary contract for any loadable module.
- Define the patterns for communication: `ICommand`, `ICommandHandler`, `IEvent`, `IEventHandler`.
- Define contracts for runtime discovery, such as `IApiEndpoint`.
- Define the `ITraceContext` for passing diagnostic information through the system.
- Provide base abstractions from `Microsoft.Extensions.*` to ensure a common language for Dependency Injection and Logging without enforcing a specific implementation.

## Architectural Principles
- **Minimalism:** Contains only what is absolutely necessary for the framework to function. It does not contain contracts for specific services like logging or installation.
- **Stability:** This assembly should change very infrequently. Changes here are breaking changes for the entire ecosystem.
- **The Shared Language:** This is the only assembly that third-party module developers should need to reference to build a compatible module.

## Project Structure
```
SoftwareCenter.Core/
├── Commands/
│   ├── ICommand.cs
│   └── ICommandHandler.cs
├── Diagnostics/
│   └── ITraceContext.cs
├── Events/
│   ├── IEvent.cs
│   └── IEventHandler.cs
├── Modules/
│   └── IModule.cs
├── Routing/
│   └── IApiEndpoint.cs
├── SoftwareCenter.Core.csproj
└── METADATA.md
```

## Dependencies
- `Microsoft.Extensions.DependencyInjection.Abstractions`
- `Microsoft.Extensions.Logging.Abstractions`