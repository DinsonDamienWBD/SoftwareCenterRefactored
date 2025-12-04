# SoftwareCenter.Kernel - Feature Manifest

**Document Status:** Final
**Project Status:** Completed and Verified

This document describes the concrete features of the `SoftwareCenter.Kernel` library, which provides the core logic and orchestration for the application, implementing the contracts from `SoftwareCenter.Core`.

---

### **1. Feature: Central Kernel Orchestrator**
- **Description:** The Kernel provides a concrete, unified implementation of the `IKernel` interface. An instance of `StandardKernel` is the central hub that is passed to all modules, giving them access to all core system services from a single object.
- **Status:** `Completed`
- **Public API Surface:** `StandardKernel`

---

### **2. Feature: Dynamic Module Loading Engine**
- **Description:** The Kernel can dynamically discover, load, and initialize modules from the filesystem at runtime. Each module is loaded into an isolated `AssemblyLoadContext` to prevent conflicts and allow for future hot-swapping capabilities.
- **Status:** `Completed`
- **Public API Surface:** `ModuleLoader`, `ModuleLoadContext`

---

### **3. Feature: Intelligent Command Routing & Prioritization**
- **Description:** The Kernel features an advanced routing system that replaces the Host's default router. It supports multiple handlers for the same command, executing only the one with the highest priority. This allows modules to gracefully override default Host or other module functionality. It also provides a safety barrier, catching exceptions from handlers and managing command lifecycles.
- **Status:** `Completed`
- **Public API Surface:** `SmartRouter`, `HandlerRegistry`

---

### **4. Feature: Comprehensive Service Discovery API**
- **Description:** The Kernel provides a "manifest" of all currently available commands in the entire system, including those from the Host and all loaded modules. This API is essential for UIs, developer tools, and AI agents to understand the system's capabilities at runtime.
- **Status:** `Completed`
- **Implementation:** The `GetRegistryManifest()` method on the `HandlerRegistry` class provides this functionality.

---

### **5. Feature: Concrete Service Implementations**
- **Description:** The Kernel provides default, concrete implementations for the primary service contracts defined in `SoftwareCenter.Core`.
- **Status:** `Completed`
- **Public API Surface:** `DefaultEventBus`, `GlobalDataStore`, `JobScheduler`, `KernelLogger`

---
---

## Appendix: Detailed API Reference

### **Namespace: `SoftwareCenter.Kernel`**

#### `public class StandardKernel : IKernel, IDisposable`
The "Composition Root" for the Brain. Implements IKernel, DI, and Module Loading.
- **Properties:**
  - `IRouter Router { get; }`
  - `IGlobalDataStore DataStore { get; }`
  - `IEventBus EventBus { get; }`
  - `IJobScheduler JobScheduler { get; }`
- **Methods:**
  - `public StandardKernel()`
  - `public Task StartAsync()`
    - **Summary:** Loads standard modules from the `./Modules` directory and logs kernel boot status.
  - `public T GetService<T>() where T : class`
    - **Summary:** Gets a registered service from the Kernel's internal DI container.
  - `public void Dispose()`
- **IKernel Implementation:**
  - `public void Register(string commandName, Func<ICommand, Task<IResult>> handler, RouteMetadata metadata)`
    - **Summary:** Registers a capability (Command Handler) with the system. Default priority is 0.
  - `public Task<IResult> RouteAsync(ICommand command)`
    - **Summary:** Implements `IRouter.RouteAsync` by delegating to the internal `SmartRouter` instance.

### **Namespace: `SoftwareCenter.Kernel.Contexts`**

#### `public class ModuleLoadContext : AssemblyLoadContext`
Provides assembly isolation for Modules. Ensures 3rd party modules don't conflict with Host dependencies and allows for unloading.
- `public ModuleLoadContext(string pluginPath)`
- `protected override Assembly? Load(AssemblyName assemblyName)`
- `protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)`

### **Namespace: `SoftwareCenter.Kernel.Engine`**

#### `public class ModuleLoader`
- `public ModuleLoader(IKernel kernel, HandlerRegistry registry)`
- `public async Task LoadModulesAsync(string modulesRootPath)`
  - **Summary:** Scans subdirectories in the given path, finds module DLLs, loads them into an isolated context, and calls their `InitializeAsync` method.

### **Namespace: `SoftwareCenter.Kernel.Routing`**

#### `public class HandlerRegistry`
Implements the Handler Registry (Service Discovery). Acts as the dynamic catalog of all capabilities and supports priority sorting.
- `public void Register(string commandId, Func<ICommand, Task<IResult>> handler, RouteMetadata metadata, int priority = 0)`
  - **Summary:** Registers a new command capability.
- `public IEnumerable<RouteMetadata> GetRegistryManifest()`
  - **Summary:** Discovery API: Returns a flat list of ALL registered handlers, not just the highest priority ones. Includes priority and active status. Used by UI to generate menus or AI to know all capabilities.
- `public void UnregisterModule(string moduleName)`
  - **Summary:** Removes handlers from a specific module (Hot-Swap support).

#### `public class SmartRouter : IRouter`
Implements the Smart Router. Acts as the central "Traffic Cop" and "Exception Barrier".
- `public SmartRouter(HandlerRegistry registry, IEventBus eventBus)`
- `public async Task<IResult> RouteAsync(ICommand command)`
  - **Summary:** Manages the entire lifecycle of a command: starts or joins a trace context, finds the best handler, checks for obsolete/deprecated status, injects trace info for logging commands, and wraps the final execution in an exception barrier to prevent app crashes.

### **Namespace: `SoftwareCenter.Kernel.Services`**

#### `public class DefaultEventBus : IEventBus`
Implements the Event Bus (Pub/Sub). An asynchronous, loosely coupled messaging system.
- `public void Subscribe(string eventName, Func<IEvent, Task> handler)`
- `public void Unsubscribe(string eventName, Func<IEvent, Task> handler)`
- `public async Task PublishAsync(IEvent systemEvent)`

#### `public class GlobalDataStore : IGlobalDataStore, IDisposable`
Implements the Global Data Store. A Hybrid Store that uses RAM for `Transient` data and LiteDB for `Persistent` data. It is thread-safe and async.
- `public GlobalDataStore()`
- `public Task<bool> StoreAsync<T>(string key, T data, DataPolicy policy = DataPolicy.Transient)`
- `public Task<DataEntry<T>?> RetrieveAsync<T>(string key)`
- `public Task<bool> ExistsAsync(string key)`
- `public Task<bool> RemoveAsync(string key)`
- `public Task<DataEntry<object>?> GetMetadataAsync(string key)`
- `public Task<bool> StoreBulkAsync<T>(IDictionary<string, T> items, DataPolicy policy = DataPolicy.Transient)`
- `public void Dispose()`

#### `public class JobScheduler : IJobScheduler, IDisposable`
Implements centralized job scheduling. Manages lifecycles, crashes, and logging for background tasks.
- `public JobScheduler(KernelLogger logger)`
- `public void Register(IJob job)`
- `public void TriggerAsync(string jobName)`
- `public void Pause(string jobName)`
- `public void Resume(string jobName)`
- `public void Dispose()`

#### `public class KernelLogger`
Implements Intelligent Logging Consumer. Observes Kernel traffic and broadcasts standardized LogEntry events.
- `public KernelLogger(IGlobalDataStore dataStore, IEventBus eventBus)`
- `public void RefreshSettings()`
  - **Summary:** Call this when settings change to invalidate the verbosity cache.
- `public async Task LogExecutionAsync(ICommand command, bool success, long durationMs, string? error = null)`
  - **Summary:** Logs the outcome of a command execution, checks verbosity settings, and publishes a `LogEvent` to the event bus.