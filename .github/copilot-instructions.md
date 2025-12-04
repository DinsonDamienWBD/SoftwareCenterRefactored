# SoftwareCenter Refactored - AI Coding Agent Instructions

## Architecture Overview

**SoftwareCenter** is a modular, microkernel-based application management platform built on .NET 8. The system uses a **command bus pattern** with dynamic module loading to decouple the Host from business logic.

### Core Components

- **Host** (`Host/`): ASP.NET Core web interface. Loads Kernel on startup via reflection. Provides UI zones (Title, Notification, Power, Navigation, Content) and manages Shadow State for offline re-hydration.
- **Kernel** (`Contract & Routing/Kernel/`, `StandardKernel.cs`): The "brain" - orchestrates all routing, module loading, and inter-component communication. Implements `IKernel`.
- **Contracts** (`Contract & Routing/Core/`): Shared interfaces and DTOs (ICommand, IResult, IRouter, IGlobalDataStore, IEventBus, IJobScheduler, IModule).
- **Modules** (`Modules/`): Feature plugins (AppManager, SourceManager, CredManager, DBManager, LogNotifManager, AI.Agent). Each implements `IModule` and registers command handlers via `kernel.Register()`.
- **Support Services** (`Additional Services/`): Launcher (process control) and InstallerService (admin-level silent installations).

### Dependency Contracts

- **Host** → references Contracts + Kernel (via reflection load).
- **Kernel** → references Contracts only (no Host, no Module references).
- **Modules** → reference Contracts + Kernel only (no Host, no other Modules direct refs).
- **All projects** → must reference `SoftwareCenter.Core.csproj` first.

## Critical Design Patterns

### 1. Command Bus (String-Based, Dictionary Parameters)

All communication uses `ICommand` with `Dictionary<string, object>` parameters (never use business DTOs in routing):

```csharp
// Example: Handler registration
kernel.Register(
    "AppManager.Install",
    async (cmd) =>
    {
        var appId = cmd.Parameters["AppId"] as string;
        // Execute
        return Result.FromSuccess(new { installed = true });
    },
    new RouteMetadata { CommandId = "AppManager.Install", SourceModule = "AppManager" }
);
```

**Why**: Eliminates compile-time coupling. Kernel never references module-specific namespaces.

### 2. Smart Router with Fallback & Deprecation

`SmartRouter` (`Contract & Routing/Kernel/Routing/SmartRouter.cs`) prioritizes handlers:

- **Priority tiers**: Host default (0) < Module (1-99) < System (100).
- **Deprecated routes**: Log warning, execute anyway.
- **Obsolete routes**: Reject with error.
- **Fallback**: If primary fails and `AllowFallback=true`, try next priority.
- **Exception barrier**: All handler execution wrapped in try-catch; errors never crash Kernel.

### 3. Trace Context & History (Distributed Tracing)

Every command carries:

- `TraceId` (Guid): Unique request identifier across entire call chain.
- `History` (List<TraceHop>): Breadcrumb trail showing "ModuleA → Sent", "Router → Routed", "ModuleB → Received".

Kernel proxy automatically injects TraceId if missing. Modules **must never manually set TraceId**.

### 4. Global Data Store (LiteDB)

- **Location**: `%AppData%/SoftwareCenter/GlobalDataStore.db`
- **API**: `await kernel.DataStore.StoreAsync(key, value, policy)` / `RetrieveAsync(key)`
- **Policies**: `Persistent` (survives restart), `Temporary` (cleared on shutdown).
- **Thread-safe**: All access is async.

### 5. Shadow State (Host Only)

Host maintains an in-memory `Dictionary<string, object>` shadow state to re-hydrate hidden UI fragments when users return. Updated via `System.UI.UpdateShadow` command from modules.

### 6. UI Structure: Template-Based with Design Tokens

Host renders 5 mandatory zones:

1. **Title**: App name, breadcrumbs.
2. **Notification**: Toast/alert area.
3. **Power**: Settings/admin panel.
4. **Navigation**: Module menu.
5. **Content**: Dynamic module UI (raw HTML/CSS from `Modules/<Name>/UI/Html`, `Script`, `Style`).

**Design Tokens**: Host exposes CSS variables at runtime via `System.Style.GetTokens` API for consistent theming.

## Project-Specific Conventions

### Build & Artifact Layout

`Directory.Build.props` centralizes output paths:

- **Host + Core + Kernel**: `bin/$(Configuration)/SoftwareCenter.Host/`
- **Modules**: `bin/$(Configuration)/SoftwareCenter.Host/Modules/<ModuleName>/`
- **InstallerService**: `bin/$(Configuration)/SoftwareCenter.InstallerService/`

**Build command**:
```pwsh
dotnet build SoftwareCenterRefactored.slnx -c Release
```

Module DLLs must land in `/Modules/` folder so `ModuleLoader.LoadModulesAsync()` discovers them.

### Metadata Organization

Every project has metadata in `Metadata/<ProjectName>/`:

- `Project_Structure.md`: File layout, folder purposes.
- `Project_Implementation_Plan.md`: Current phase, blockers, TODOs.
- `Project_History_Log.md`: Git commit summary + code changes.

**Rule**: Before starting any coding phase, sync these 3 docs. Update History Log at phase end with changes, test status, blockers.

### Module Lifecycle

1. **Load**: Host reflection-loads DLL from `Modules/<ModuleName>/`.
2. **Initialize**: `ModuleLoader` calls `module.InitializeAsync(kernel)`. Module registers handlers.
3. **Running**: Handlers execute via `SmartRouter.RouteAsync()`.
4. **Unload**: On shutdown, `module.UnloadAsync()` called to release resources (DB connections, file handles, timers).

**Stub implementation**:

```csharp
public class MyModule : IModule
{
    public string ModuleName => "MyModule";
    public string Version => "1.0.0";
    public bool IsInitialized { get; private set; }

    public async Task InitializeAsync(IKernel kernel)
    {
        kernel.Register("MyModule.DoSomething", async (cmd) =>
        {
            // Implementation
            return Result.FromSuccess(data);
        }, new RouteMetadata { CommandId = "MyModule.DoSomething", SourceModule = "MyModule" });
        IsInitialized = true;
    }

    public async Task UnloadAsync()
    {
        // Clean up
        IsInitialized = false;
    }
}
```

### API Route Convention

Host exposes module capabilities via `/api/<route>` endpoints. Routes map to Kernel commands:

- `/api/src/copy` → `"SourceManager.Copy"`
- `/api/cred/save` → `"CredManager.Save"`
- `/api/mdl/coms` → `"AppManager.GetCommands"` (discovery)
- `/api/docs` → `"System.Help"` (Kernel system command)

System always returns:

```json
{
  "success": true,
  "data": { /* payload */ },
  "responder": "ModuleName",
  "trace": "guid"
}
```

### Logging Matrix

Logs have **two dimensions**:

- **Level**: Info, Warn, Error (standard).
- **Verbosity**: Important (always shown), Verbose (debug, configurable).

Host basic logger (Priority 0) writes `Level.Important` to file. LogNotifManager (Priority 100) overrides with Serilog sink to Application Insights if `System.Log.Config` set `Verbose=true`.

### Error Handling Pattern

**Never crash the Kernel**. All handler code uses try-catch internally:

```csharp
public async Task<IResult> HandleAsync(ICommand cmd)
{
    try
    {
        // Business logic
        return Result.FromSuccess(data);
    }
    catch (Exception ex)
    {
        // Log, then fail gracefully
        await kernel.DataStore.StoreAsync($"Error:{cmd.Name}:{DateTime.Now}", ex.Message, DataPolicy.Persistent);
        return Result.FromFailure($"Operation failed: {ex.Message}");
    }
}
```

Host UI always receives an `IResult` (never an unhandled exception).

## Critical Rules for This Project

1. **No UI logic in Kernel**: Kernel is headless. All UI rendering lives in Host or Module UI folders.
2. **No Host references in Kernel**: Use IKernel interface only. Kernel must be Host-agnostic.
3. **Async-first**: All command handlers are `Task<IResult>`. Never block the UI.
4. **Contract stability**: Existing command signatures in `ICommand`, `IResult` cannot change. Add new members only (Rule 11).
5. **Green build**: Keep code compiling after every change. Commit frequently.
6. **Thread-safety**: All shared state (DataStore, EventBus, Registry) must be thread-safe or protected.
7. **Module isolation**: Use `AssemblyLoadContext` for each module to prevent DLL hell.

## Workflow for Adding a Feature

1. **Plan**: Create or update `Metadata/<ProjectName>/Project_Implementation_Plan.md` with phase steps.
2. **Code**: Implement feature in target project, follow existing patterns.
3. **Register**: If new command, call `kernel.Register()` during module `InitializeAsync()`.
4. **Test**: Verify via Host API endpoint (e.g., Postman POST `/api/<route>`).
5. **Update Metadata**: Add entry to `Project_History_Log.md` with commit hash, changes, test status.
6. **Commit**: Push to `master` with message referencing project/phase.

## Key Files to Reference

- **Architecture**: `Metadata/Global/Project_Overview_Compressed.md`
- **Kernel impl**: `Contract & Routing/Kernel/StandardKernel.cs`
- **Router logic**: `Contract & Routing/Kernel/Routing/SmartRouter.cs`
- **Data store**: `Contract & Routing/Kernel/Services/GlobalDataStore.cs`
- **Module loader**: `Contract & Routing/Kernel/Engine/ModuleLoader.cs`
- **Host entry**: `Host/Program.cs`
- **Sample module metadata**: `Metadata/Global/Samples/Sample_Project_History_Log.md`

## Tools & Environment

- **Language**: C# 12, .NET 8
- **Database**: LiteDB (embedded, no external dependency)
- **Build**: `dotnet build`, `dotnet publish` (via Directory.Build.props)
- **Version control**: Git, master branch
- **IDE**: Visual Studio 2022+ recommended for solution file (`SoftwareCenterRefactored.slnx`)
