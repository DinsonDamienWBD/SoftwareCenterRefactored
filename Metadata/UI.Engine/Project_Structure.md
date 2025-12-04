# Project: SoftwareCenter.UI.Engine - Structure

## High-Level Architecture

`SoftwareCenter.UI.Engine` is a mandatory, core component of the SoftwareCenter application, functioning as a "UI-Kernel". It is responsible for the composition, state management, and rendering logic of the entire user interface. It provides a centralized, backend-driven UI service to the `Host` and all loaded modules.

This project follows a backend-driven UI approach. The server-side engine maintains a complete model of the UI, and the frontend (a browser client) acts as a thin rendering layer that displays the UI based on real-time instructions from the server.

## Core Components

1.  **`IUiCompositionService` (Interface)**:
    *   The primary contract for interacting with the UI Engine.
    *   Exposes methods for creating, updating, and removing UI elements (Navigation Buttons, Content Containers, Cards, Controls).
    *   Provides a mechanism for components to request UI real estate and define ownership and permissions.

2.  **`UiCompositionService` (Implementation)**:
    *   The server-side implementation of `IUiCompositionService`.
    *   Maintains an in-memory, tree-like representation of the application's complete UI state.
    *   Manages the lifecycle of all UI elements, including generating unique IDs and enforcing ownership/permission rules.

3.  **UI Element Models (Data Contracts)**:
    *   A set of C# classes/records that define the structure and properties of all available UI components (e.g., `NavigationButton`, `Card`, `Label`, `Button`). These models are serialized and sent to the client for rendering.

4.  **`UiHub` (SignalR Hub)**:
    *   The real-time communication bridge between the `UiCompositionService` and the browser client.
    *   When the UI state changes on the server, the `UiCompositionService` invokes methods on the `UiHub`.
    *   The `UiHub` pushes these changes to all connected clients, instructing them to add, remove, or update DOM elements.
    *   It also receives messages from the client, such as user interaction events (e.g., a button click), and forwards them to the appropriate backend service via the Kernel.

## Relationship with Other Projects

*   **`Host`**: The Host project starts the Kestrel web server and registers the `UiCompositionService` and `UiHub`. It serves the initial `index.html` and JavaScript files that form the client-side rendering shell. The Host itself uses the `IUiCompositionService` to render its own basic UI components.
*   **`Kernel`**: The Kernel ensures the `UI.Engine` is loaded and makes the `IUiCompositionService` available to all other modules via dependency injection.
*   **Modules**: Any module that requires a user interface will get an instance of `IUiCompositionService` from the Kernel and use it to request and manage its UI elements.
