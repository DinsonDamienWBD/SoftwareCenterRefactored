# Kernel Project Implementation Plan

## Phase 1: Module Loading & Routing (Completed)
- **Objective:** Implement the core responsibilities of the Kernel.
- **Status:** Completed.
- **Key Outcomes:**
    - `StandardKernel` class that implements `IKernel`.
    - Dynamic loading of `IModule` assemblies from disk.
    - A command handler registry for routing `ICommand` objects.

## Phase 2: Service Locator Implementation (Current)
- **Objective:** Implement the service location methods added to the `IKernel` interface.
- **Status:** In Progress.
- **Plan:**
    - **Step 2.1: Implement `RegisterService` and `GetService`:** In `StandardKernel.cs`, add the implementation for these two methods. This will be backed by a `private readonly Dictionary<Type, object> _services` field to store and retrieve singleton service instances.
