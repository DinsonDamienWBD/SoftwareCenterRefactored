# SoftwareCenter.UI.Engine

**Owner:** `SoftwareCenter.Host`
**Version:** 1.0.0

## Description

The UI.Engine is a mandatory, core component responsible for dynamically composing the application's user interface. It operates on a request-response model, where the Host and other modules request UI elements, and the engine renders them in the browser client. It leverages a local Kestrel web server and communicates with the frontend via WebSockets (SignalR) for real-time updates.

## Dependencies

- `SoftwareCenter.Core`: For access to shared contracts like `IUIEngine` and UI data models.

## Implementation Plan

### Phase 1: Core Contract Definition (Completed)

- **`SoftwareCenter.Core`**:
  - **Done:** Defined `IUIEngine` interface in `SoftwareCenter.Core.UI`.
  - **Done:** Defined request DTOs (`NavButtonRequest`, `ContentContainerRequest`, `UIUpdateRequest`) in `SoftwareCenter.Core.UI.Requests`.

### Phase 2: Project Scaffolding (Completed)

- **`SoftwareCenter.UI.Engine`**:
  - **Done:** Created the `SoftwareCenter.UI.Engine.csproj` file.
  - **Done:** Added a project reference to `SoftwareCenter.Core`.
  - **Done:** Created the initial `UIEngine.cs` class implementing `IUIEngine` with placeholder logic.

### Phase 3: Host Integration & Basic Web Server Setup

- **`SoftwareCenter.Host`**:
  - Modify `Program.cs` to build and run a Kestrel web server.
  - Configure static file serving for HTML/CSS/JS.
  - Add a project reference to `SoftwareCenter.UI.Engine`.
  - Instantiate `UIEngine` and register it with the DI container.
- **`SoftwareCenter.UI.Engine`**:
  - Create a basic `index.html` file to act as the Single Page Application (SPA) shell.
  - Create basic CSS and JS files.

### Phase 4: Real-time Communication & Basic Rendering

- **`SoftwareCenter.Host`**:
  - Add SignalR services to the DI container.
  - Map a SignalR hub endpoint.
- **`SoftwareCenter.Core`**:
  - Update `IModule` to include an optional `SpaUrl` property.
- **`SoftwareCenter.UI.Engine`**:
  - Create a `UIHub` class that inherits from `Microsoft.AspNetCore.SignalR.Hub`.
  - Update `UIEngine` service to inject `IHubContext<UIHub>` to communicate with the client.
  - Implement the `IUIEngine` methods to call hub methods on the client (e.g., `Clients.All.SendAsync("ReceiveNavButton", ...)`)
- **Frontend (`wwwroot/js`)**:
  - Add the SignalR client library.
  - Write JavaScript to connect to the `UIHub`.
  - Implement client-side handlers (`connection.on(...)`) to receive UI commands and dynamically create/update DOM elements using the data received from the engine.

### Phase 5: Advanced UI - Card Interface & Module SPAs

- **`SoftwareCenter.Core`**:
  - Define data contracts for the Card Interface (`Card`, `CardUpdateRequest`, etc.).
- **`SoftwareCenter.UI.Engine`**:
  - Implement the server-side logic for managing card-based layouts within containers.
  - Implement the logic to automatically add a "pop-out" button for modules that register a `SpaUrl`.
- **Frontend (`wwwroot/js`)**:
  - Implement the JavaScript rendering logic for the card interface.
  - Implement the "pop-out" button functionality.
- **`SoftwareCenter.Host`**:
  - Implement a middleware or routing convention to serve static files for module SPAs (e.g., from a `/modules/{moduleName}` path).

### Phase 6: UI Interaction & Ownership

- **`SoftwareCenter.Host`**:
  - Create a generic API endpoint (e.g., `/api/ui/interact`) to receive UI interaction events from the client.
  - Use the Kernel's routing to forward these interaction requests to the correct owning module.
- **Frontend (`wwwroot/js`)**:
  - Implement client-side logic to capture user interactions (e.g., button clicks) within module-owned UI.
  - This logic will read `data-*` attributes to get the owner ID and action details, then send a request to the interaction API endpoint.