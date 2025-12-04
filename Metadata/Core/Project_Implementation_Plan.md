# SoftwareCenter.Core - Feature Manifest

**Document Status:** Final
**Project Status:** Completed and Verified

This document describes the public-facing features and contracts of the `SoftwareCenter.Core` library. Developers should use this as the primary reference for understanding the library's capabilities.

---

### **1. Feature: Command & Result Pattern**
- **Description:** A CQRS-inspired pattern for all imperative actions in the system. Commands are requests for the system to do something, and Results are the synchronous outcome of that request. This enforces a clear, traceable, and standardized flow for all operations.
- **Status:** `Completed`
- **Public API Surface:** `ICommand`, `IResult`, `Result`

---

### **2. Feature: Ambient Traceability Context**
- **Description:** A system that automatically links all actions (commands, events, logs) originating from a single top-level operation. It works "ambiently" across `async/await` calls, so developers do not need to manually pass trace IDs.
- **Status:** `Completed`
- **Public API Surface:** `TraceContext`, `TraceHop`

---

### **3. Feature: Module & Kernel Contracts**
- **Description:** Defines the relationship between the central application (`Kernel`) and its external plugins (`Modules`). A module is a self-contained package of functionality that registers itself with the Kernel upon loading.
- **Status:** `Completed`
- **Public API Surface:** `IModule`, `IKernel`

---

### **4. Feature: Decoupled Event Bus**
- **Description:** A publish-subscribe system that allows components to broadcast informational messages (`Events`) without being coupled to the recipients. This is ideal for notifications and reactive workflows.
- **Status:** `Completed`
- **Public API Surface:** `IEvent`, `IEventBus`

---

### **5. Feature: Service Discovery & Routing**
- **Description:** Defines the contracts for how `Commands` are dynamically routed to handler implementations. The system supports multiple handlers for the same command, prioritized to allow modules to override default behavior. The `RouteMetadata` object provides a rich manifest for developer tooling and UIs.
- **Status:** `Completed`
- **Public API Surface:** `IRouter`, `RouteMetadata`, `RouteStatus`

---

### **6. Feature: Global Shared Data Store**
- **Description:** A contract for a simple, application-wide key-value storage service. It allows different modules to share data with defined persistence policies and automatic trace metadata.
- **Status:** `Completed`
- **Public API Surface:** `IGlobalDataStore`, `DataEntry<T>`, `DataPolicy`

---

### **7. Feature: Abstract UI Composition**
- **Description:** Provides a set of simple contracts for building a modular UI where different components can provide UI parts to be rendered in designated zones of the main application shell.
- **Status:** `Completed`
- **Public API Surface:** `ContentPart`, `UIControl`, `UIZone`

---

### **8. Feature: Background Job Scheduling**
- **Description:** A set of contracts for defining and scheduling background or long-running tasks.
- **Status:** `Completed`
- **Public API Surface:** `IJob`, `JobContext`, `IJobScheduler`

---
---

## Appendix: Detailed API Reference

### **Namespace: `SoftwareCenter.Core.Commands`**

#### `public interface ICommand`
The universal envelope for all actions in the system. Used to decouple the Host from the Kernel and Modules.
- `string Name { get; }`
  - **Summary:** The unique identifier for the action (e.g., "Module.Install", "Weather.Get").
- `Dictionary<string, object> Parameters { get; }`
  - **Summary:** The payload for the command. Key: Parameter name (e.g., "FilePath"). Value: The data (e.g., "C:/Temp/file.zip").
- `Guid TraceId { get; }`
  - **Summary:** TRACING
- `List<TraceHop> History { get; }`
  - **Summary:** The Audit Trail. The Kernel's Proxy automatically adds to this list. Modules generally do not touch this.

#### `public interface IResult`
The universal response envelope. Every command must return this, ensuring the app never crashes from unhandled exceptions.
- `bool Success { get; }`
  - **Summary:** True if the operation completed successfully.
- `string Message { get; }`
  - **Summary:** Human-readable feedback. If Success is false, this contains the error message.
- `object? Data { get; }`
  - **Summary:** The return payload. Can be null if the operation has no return value. The receiver is responsible for casting this to the expected type.
- `Guid TraceId { get; }`
  - **Summary:** TRACING
- `List<TraceHop> History { get; }`
  - **Summary:** The Audit Trail.

#### `public class Result : IResult`
Concrete implementation of IResult. Provides static factory methods for convenient creation of success/failure responses. Automatically binds to the current TraceContext.
- `public static Result FromSuccess()`
  - **Summary:** Creates a successful result with no data.
- `public static Result FromSuccess(object? data = null, string message = "Operation successful.")`
  - **Summary:** Creates a successful result with a payload.
  - **Parameters:**
    - `data`: The return value (e.g., a List of Files).
    - `message`: Optional success message.
- `public static Result FromFailure(string message)`
  - **Summary:** Creates a failure result.
  - **Parameters:** `message`: Error description.
- `public static Result FromFailure(Exception ex)`
  - **Summary:** Creates a failure result from an Exception.
  - **Parameters:** `ex`: The exception that caused the failure.

### **Namespace: `SoftwareCenter.Core.Data`**

#### `public enum DataPolicy`
Dictates how the Global Data Store handles persistence for a specific key.
- `Transient = 0`
  - **Summary:** Stored in RAM only. Lost when the application closes. Use for: Session tokens, temporary UI state, caching.
- `Persistent = 1`
  - **Summary:** Saved to disk (e.g., LiteDB/SQLite). Survives restarts. Use for: User settings, application config, long-term logs.

#### `public class DataEntry<T>`
A rich wrapper around stored data. Provides accountability (Who saved this?) and validity checks (When was this saved?).
- `T? Value { get; set; }`
  - **Summary:** The actual data payload.
- `DateTime LastUpdated { get; set; }`
  - **Summary:** UTC Timestamp of the last write operation.
- `string SourceId { get; set; }`
  - **Summary:** The ID of the module that owns/wrote this data.
- `string DataType { get; set; }`
  - **Summary:** The fully qualified type name of the data (safety check for deserialization).
- `Guid TraceId { get; set; }`
  - **Summary:** The Trace ID of the command/operation that caused this data change.

#### `public interface IGlobalDataStore`
The contract for the system's "Synapse" (Shared Memory). Abstracts away the physical storage (RAM vs Disk/DB).
- `Task<bool> StoreAsync<T>(string key, T data, DataPolicy policy = DataPolicy.Transient)`
  - **Summary:** Stores data with a specific persistence policy.
- `Task<DataEntry<T>?> RetrieveAsync<T>(string key)`
  - **Summary:** Retrieves the data along with its metadata (timestamp, source).
- `Task<bool> ExistsAsync(string key)`
  - **Summary:** Checks if a key exists without incurring the cost of deserializing the payload.
- `Task<bool> RemoveAsync(string key)`
  - **Summary:** Removes a specific key from storage.
- `Task<DataEntry<object>?> GetMetadataAsync(string key)`
  - **Summary:** Retrieves only the metadata (Header) without loading the full payload.
- `Task<bool> StoreBulkAsync<T>(IDictionary<string, T> items, DataPolicy policy = DataPolicy.Transient)`
  - **Summary:** Stores a collection of data in a single transaction.

### **Namespace: `SoftwareCenter.Core.Diagnostics`**

#### `public struct TraceHop`
Represents a single step in the journey of a request. Immutable struct for performance.
- `DateTime Timestamp { get; }`
- `string EntityId { get; }` (e.g., "Host", "Kernel", "GitModule")
- `string Action { get; }` (e.g., "Sent", "Received", "Published")

#### `public class TraceContext`
Represents a Trace Session. It contains static members to manage the 'Ambient' (Thread-Local) context.
- `Guid TraceId { get; set; }`
- `List<TraceHop> History { get; }`
- `void AddHop(string entityId, string action)`
- `static TraceContext? Current { get; set; }`
  - **Summary:** Gets or sets the context for the current async flow.
- `static Guid? CurrentTraceId { get; set; }`
  - **Summary:** Helper: Returns the ID of the current context, or null if none exists. Setting this ensures a Context object exists.
- `static TraceContext StartNew()`
  - **Summary:** Starts a fresh trace for the current thread.

### **Namespace: `SoftwareCenter.Core.Events`**

#### `public interface IEvent`
Represents a system-wide broadcast. Unlike Commands (1-to-1), Events are 1-to-Many.
- `string Name { get; }`
  - **Summary:** The topic/name of the event (e.g., "Job.Failed", "Download.Progress").
- `Dictionary<string, object> Data { get; }`
  - **Summary:** The event payload.
- `DateTime Timestamp { get; }`
  - **Summary:** When the event occurred (UTC).
- `Guid? TraceId { get; }`
  - **Summary:** Connects this event to the Command that caused it.
- `string SourceId { get; }`
  - **Summary:** Populated automatically by the Kernel Proxy.

#### `public interface IEventBus`
Defines the Pub/Sub mechanism.
- `Task PublishAsync(IEvent systemEvent)`
  - **Summary:** Broadcasts an event to all subscribers of that event name.
- `void Subscribe(string eventName, Func<IEvent, Task> handler)`
  - **Summary:** Subscribes a handler to a specific event topic.
- `void Unsubscribe(string eventName, Func<IEvent, Task> handler)`
  - **Summary:** Unsubscribes a handler to prevent memory leaks.

### **Namespace: `SoftwareCenter.Core.Jobs`**

#### `public interface IJob`
Represents a recurring background task provided by a Module.
- `string Name { get; }`
  - **Summary:** Unique identifier (e.g., "BackupModule.DailyBackup").
- `TimeSpan Interval { get; }`
  - **Summary:** How often the job should run.
- `Task ExecuteAsync(JobContext context)`
  - **Summary:** The logic to execute.

#### `public class JobContext`
Context passed to a running job. Provides traceability and cancellation support.
- `TraceContext Trace { get; set; }`
- `DateTime LastRun { get; set; }`
- `CancellationToken CancellationToken { get; set; }`
- `Dictionary<string, object> State { get; set; }`

#### `public interface IJobScheduler`
Contract for the Centralized Scheduler. Allows modules to register, pause, or manually trigger jobs.
- `void Register(IJob job)`
- `void TriggerAsync(string jobName)`
  - **Summary:** Manually runs a job immediately.
- `void Pause(string jobName)`
- `void Resume(string jobName)`

### **Namespace: `SoftwareCenter.Core.Kernel`**

#### `public interface IKernel : IRouter`
The specific contract exposed to Modules during the Initialize phase. Aggregates Routing, Event Bus, and Data Store capabilities.
- `IEventBus EventBus { get; }`
- `IGlobalDataStore DataStore { get; }`
- `IJobScheduler JobScheduler { get; }`
- `void Register(string commandName, Func<ICommand, Task<IResult>> handler, RouteMetadata metadata)`
  - **Summary:** Registers a capability (Command Handler) with the system.

### **Namespace: `SoftwareCenter.Core.Logging`**

#### `public enum LogLevel`
- `Debug`, `Info`, `Warning`, `Error`, `Critical`

#### `public class LogEntry`
- `DateTime Timestamp { get; set; }`
- `LogLevel Level { get; set; }`
- `string Message { get; set; }`
- `string Source { get; set; }`
  - **Summary:** The component issuing the log (e.g., "GitModule").
- `string Category { get; set; }`
  - **Summary:** High-level context (e.g. "Command.Execution", "System.Boot").
- `Guid? TraceId { get; set; }`
  - **Summary:** The trace ID of the transaction active when this log was created.
- `Dictionary<string, object> ExtendedData { get; set; }`
  - **Summary:** Extra data for structured logging.

### **Namespace: `SoftwareCenter.Core.Modules`**

#### `public interface IModule`
The contract every feature module must implement.
- `Task InitializeAsync(IKernel kernel)`
  - **Summary:** Called immediately after the module is loaded. Use this to register routes, open database connections, or load configurations.
- `Task UnloadAsync()`
  - **Summary:** Called before the module is disposed. CRITICAL: You must release file handles, stop timers, and close DB connections here.
- `string ModuleName { get; }`
- `string Version { get; }`
- `bool IsInitialized { get; }`

### **Namespace: `SoftwareCenter.Core.Routing`**

#### `public interface IRouter`
Defines the bridge between the Host and the Kernel.
- `Task<IResult> RouteAsync(ICommand command)`
  - **Summary:** Routes a command asynchronously to the appropriate handler (Kernel or Module).

#### `public enum RouteStatus`
- `Active`, `Deprecated`, `Obsolete`, `Experimental`

#### `public class RouteMetadata`
Defines the capabilities and status of a registered command.
- `string CommandId { get; set; }`
  - **Summary:** The unique command key (e.g., "User.Save").
- `string Description { get; set; }`
  - **Summary:** Human-readable explanation.
- `string Version { get; set; }`
- `RouteStatus Status { get; set; }`
- `string? DeprecationMessage { get; set; }`
  - **Summary:** Advisory message (e.g., "Use 'User.SaveV2' instead.").
- `string SourceModule { get; set; }`
  - **Summary:** The ID of the module providing this implementation.
- `int Priority { get; set; }`
  - **Summary:** The priority of this specific handler implementation. Higher numbers are invoked first.
- `bool IsActiveSelection { get; set; }`
  - **Summary:** True if this handler is the current highest-priority active handler for the command.

### **Namespace: `SoftwareCenter.Core.UI`**

#### `public enum UIZone`
Defines the 5 mandatory zones of the layout.
- `Title`, `Notification`, `Power`, `Navigation`, `Content`

#### `public class ContentPart`
A generic container for a UI region. Represents a "Packet" of UI sent from a Module to the Host.
- `string SourceId { get; set; }`
- `Guid? TraceId { get; set; }`
- `UIZone TargetZone { get; set; }`
- `string? RegionName { get; set; }`
- `int Priority { get; set; }`
- `Guid ViewId { get; set; }`
- `string? MetaTitle { get; set; }`
- `List<UIControl> Controls { get; set; }`

#### `public class UIControl`
Defines a single UI element abstractly. The Host converts these into native widgets.
- `string ControlType { get; set; }`
  - **Summary:** The type of widget to render (e.g., "Button", "Label").
- `string Text { get; set; }`
- `string? Icon { get; set; }`
- `bool IsVisible { get; set; }`
- `bool IsEnabled { get; set; }`
- `string? BindKey { get; set; }`
  - **Summary:** The key used to bind this control's value to the generic Data Dictionary for reactive UI.
- `Dictionary<string, string> Actions { get; set; }`
  - **Summary:** Defines interactivity. Key: Event Name ("Click"). Value: Command Name to trigger ("Module.Save").
- `List<UIControl> Children { get; set; }`
  - **Summary:** Container for nested controls.
- `Dictionary<string, object> Styles { get; set; }`
  - **Summary:** Optional hints for the renderer (e.g., { "Color", "Red" }).