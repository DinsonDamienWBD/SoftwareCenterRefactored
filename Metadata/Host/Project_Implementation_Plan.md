# Software Center Host - Implementation Plan

*Architectural Note: The UI Composition Engine is a required, foundational component. It will be implemented in a dedicated, essential module (`SoftwareCenter.Module.UIManager`). The Host will not contain any rendering logic itself; it will act as a client to the UIManager for its own UI needs, just like any other module. The Host MUST verify that the UIManager DLL is present at startup.*
---

### **1. Feature: Kestrel Web Server Application**
*   **Description:** The Host will be a lightweight, self-hosted ASP.NET Core web application using the Kestrel server. It will serve a Single Page Application (SPA) built with HTML/CSS/JS from its `wwwroot` folder. Upon starting, it should automatically open the user's default web browser to the correct local URL.
*   **Implementation Plan:**
    *   Modify `Program.cs` to use the `WebApplication.CreateBuilder` model.
    *   Configure the app to serve static files from `wwwroot` and open a browser on launch.
    *   Configure Kestrel to listen on a specific local port (e.g., `http://localhost:5000`).
    *   Implement a WebSocket endpoint (e.g., `/ws`) for real-time, two-way communication.

### **2. Feature: Kernel & Framework Pre-flight Checks**
*   **Description:** Before starting the Kernel, the Host must verify that all critical framework components are present. If any required DLL is missing, the application must fail immediately with a descriptive error and stop.
*   **Implementation Plan:**
    *   In `Program.cs`, before initializing the Kernel, add a check for the existence of:
        *   `SoftwareCenter.Core.dll`
        *   `SoftwareCenter.Kernel.dll`
        *   `SoftwareCenter.Module.UIManager.dll` (in the modules directory)
    *   If a file is missing, log a fatal error to the console and terminate the application.

### **3. Feature: Kernel Lifecycle Management (The "Handshake")**
*   **Description:** The Host is responsible for finding, loading, and initializing the `SoftwareCenter.Kernel`. It will then hand over primary control of the application logic to the Kernel.
*   **Implementation Plan:**
    *   Create a dedicated `Logic/KernelManager.cs` class to manage the `StandardKernel` instance.
    *   Expose the `IKernel` instance as a singleton for use throughout the Host.
    *   After the pre-flight checks pass, call `kernel.StartAsync()` from `Program.cs` to trigger the loading of all services and modules (including the UIManager).

### **4. Feature: Host-Native Services & UI Registration**
*   **Description:** The Host provides baseline, "local-only" services and a basic UI for them. It registers both its command handlers and its UI requests with the Kernel, acting as a standard client to the core frameworks.
*   **Implementation Plan:**
    *   **Native Service Handlers:** Implement handlers for basic logging, local file operations, etc., in the `/Services` directory.
    *   **Host Baseline UI:** For its features (e.g., local source management), the Host will create standard `ContentPart` objects and send `UI.Request.Nav` commands to the Kernel at startup. These will be handled by the UIManager module.
    *   **Registration:** Use a `RegisterHostServices(IKernel kernel)` method, called after the kernel is ready, to register all Host-provided command handlers and to send its UI registration commands. Handlers will be registered with a low priority (`Priority: 0`).

### **5. Feature: WebSocket Frontend-Backend Bridge**
*   **Description:** A persistent, two-way communication channel using WebSockets is required for the backend (C#) and frontend (JS) to communicate.
*   **Implementation Plan:**
    *   Create `Middleware/WebSocketManagerMiddleware.cs` to manage the connection lifecycle.
    *   **JS -> C#:** The frontend sends command JSON objects over the WebSocket. The middleware receives these, deserializes them into `ICommand`s, and passes them to `kernel.RouteAsync()`.
    *   **C# -> JS:** Services (like the UIManager) will send messages to the frontend via this WebSocket connection to render UI, show notifications, etc.

---
---

## Appendix: Placeholder for UIManager Module Plan

The following is a placeholder for the features that will be implemented in the required `SoftwareCenter.Module.UIManager` project.

### **Feature: Centralized UI Composition Engine**
*   **Description:** The UIManager is the sole and authoritative "smart" engine responsible for the entire UI experience. It fields requests for UI components from all modules (and the Host), manages element lifecycles, ownership, and permissions, and composes the final view that gets sent to the browser via the Host's WebSocket bridge.
*   **Implementation Plan:**
    *   **Command Handling:** Register high-priority handlers for all `UI.*` commands (`UI.Request.Nav`, `UI.Request.Card`, `UI.Update.Control`, `UI.Set.Ownership`, etc.).
    *   **Element Registry:** Maintain an in-memory database of all UI elements, their unique IDs, properties, and ownership/permission metadata.
    *   **Rendering Strategy:** Implement logic to handle the different rendering modes (cards, direct controls, custom HTML). This service will generate the appropriate HTML/JSON to be sent to the frontend.
    *   **State Management:** Track the state of the UI and handle targeted updates, sending granular rendering instructions to the frontend to update only the necessary parts of the DOM.
    *   **Ownership API:** Implement the logic for transferring and sharing ownership of UI elements as requested by modules.