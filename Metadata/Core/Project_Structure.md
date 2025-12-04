# SoftwareCenter.Core - Final Project Structure

This document outlines the final, verified structure of the `SoftwareCenter.Core` project. This project contains only contracts (interfaces, enums, and data transfer objects) and no concrete logic. It serves as the central, stable foundation upon which all other components (Kernel, Host, Modules) are built.

## Project Files

- **SoftwareCenter.Core.csproj**: The .NET project file defining dependencies and build settings.

## Namespaces & Files

### `SoftwareCenter.Core.Commands`
This namespace defines the primary messaging pattern for imperative actions.

- **/ICommand.cs**: The fundamental contract for a message that requests an action. It is designed to be self-contained and traceable.
- **/IResult.cs**: The contract for the outcome of a command, indicating success or failure and carrying resulting data.
- **/Result.cs**: A concrete, generic implementation of `IResult<T>` for common use cases.

### `SoftwareCenter.Core.Data`
This namespace provides contracts for a shared data storage mechanism.

- **/IGlobalDataStore.cs**: An interface for a key-value store, allowing modules to share data with defined persistence policies.
- **/DataEntry.cs**: A wrapper for stored data that includes essential metadata like timestamps, source module, and the trace ID of the command that created it.
- **/DataPolicy.cs**: An enum (`Transient`, `Persistent`) defining the desired lifespan of a `DataEntry`.

### `SoftwareCenter.Core.Diagnostics`
This namespace contains the core components for system-wide traceability.

- **/TraceContext.cs**: Defines the ambient context for a single operation, allowing the system to track causality across different components and async calls using `AsyncLocal<T>`.
- **/TraceHop.cs**: Represents a single step in an operation's lifecycle, creating a breadcrumb trail for debugging.

### `SoftwareCenter.Core.Events`
This namespace defines the contracts for a reactive, decoupled messaging system.

- **/IEvent.cs**: The contract for a message that announces something has happened, without expectation of a direct response.
- **/IEventBus.cs**: The contract for a publish-subscribe service that routes events to interested listeners.

### `SoftwareCenter.Core.Jobs`
This namespace provides contracts for background, scheduled, or long-running tasks.

- **/JobInterfaces.cs**: Contains interfaces like `IJob` (a task to be run), `IScheduler` (for scheduling jobs), and `ITrigger` (for defining when a job should run).

### `SoftwareCenter.Core.Kernel`
This namespace defines the master contract for the central application coordinator.

- **/IKernel.cs**: The core interface that brings all other contracts together. It acts as the central point of access for modules to interact with the system, exposing the event bus, router, and module registration capabilities.

### `SoftwareCenter.Core.Logging`
This namespace provides simple, standardized contracts for logging.

- **/LogEntry.cs**: A DTO for log messages, containing the message, level, timestamp, and associated trace information.
- **/LogLevel.cs**: An enum (`Debug`, `Info`, `Warning`, `Error`, `Fatal`) for classifying log messages.

### `SoftwareCenter.Core.Modules`
This namespace defines the contract for external, dynamically-loaded application extensions.

- **/IModule.cs**: The single most important contract for a plugin. It defines the `InitializeAsync` method, which is the entry point for a module to register its commands, events, and UI components with the `IKernel`.

### `SoftwareCenter.Core.Routing`
This namespace defines how commands are registered and discovered.

- **/IRouter.cs**: The contract for registering and unregistering command handlers.
- **/RouteMetadata.cs**: A rich DTO that serves as the public manifest for a command handler. It includes descriptions, versioning, status, and, critically, the handler's `Priority` and a flag for `IsActiveSelection`. This is the primary object used for service discovery.

### `SoftwareCenter.Core.UI`
This namespace provides abstract contracts for composing a user interface.

- **/ContentPart.cs**: A generic container for a piece of UI, which can be targeted at a specific `UIZone`.
- **/UIControl.cs**: A base class or contract for UI elements.
- **/UIZone.cs**: An enum defining the major layout areas of the application shell (e.g., `Header`, `Content`, `Footer`).
