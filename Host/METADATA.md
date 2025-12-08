# Project: SoftwareCenter.Host

## Purpose
`SoftwareCenter.Host` is the main executable for the application. It configures and runs the Kestrel web server, initializes the `Kernel` and `UIManager`, and provides the default, baseline implementation for all required application services.

## Key Responsibilities
- **Application Entry Point:** Contains the `Program.main` method that starts the entire application.
- **Web Server Configuration:** Sets up and runs the ASP.NET Core Kestrel server.
- **System Initialization:** Is responsible for adding the `Kernel` and `UIManager` to the service collection and triggering the module loading process.
- **Default Service Provider:** Implements and registers basic, "good enough" versions of core services:
    - **Logging:** A simple logger that writes to a local text file.
    - **Installation:** A handler that executes the default OS action for a file (e.g., double-clicking an `.msi`).
    - **Repository:** A service that can list applications from a local folder.
- **Base UI Layout:** Sends the initial commands to the `UIManager` to construct the main application window, navigation, and content areas.

## Architectural Principles
- **Standalone Application:** The Host can run and provide core functionality without any modules present.
- **No Module References:** The Host has no compile-time knowledge of any specific module.
- **No Core Reference:** The Host does not reference `SoftwareCenter.Core` directly. It interacts with the system via the concrete `Kernel` and `UIManager` projects, which abstracts away the core contracts. This enforces a clean architectural boundary.
- **Extensible:** Its default service registrations can be overridden by modules that register themselves with a higher priority in the Kernel.

## Project Structure
```
SoftwareCenter.Host/
├── Services/
│   ├── DefaultFileLoggerProvider.cs
│   └── DefaultInstallCommandHandler.cs
├── wwwroot/
│   ├── index.html
│   └── ... (css, js)
├── Program.cs
├── SoftwareCenter.Host.csproj
└── METADATA.md
```

## Dependencies
- `SoftwareCenter.Kernel`
- `SoftwareCenter.UIManager`