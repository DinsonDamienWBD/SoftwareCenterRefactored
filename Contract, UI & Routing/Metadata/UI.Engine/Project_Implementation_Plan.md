# Project: SoftwareCenter.UI.Engine - Implementation Plan

This document outlines the phased development plan for the `SoftwareCenter.UI.Engine` project, also known as the "UI-Kernel".

## Phase 1: Core Contracts & Data Structures

*   **Objective:** Define the fundamental interfaces and data models for the entire UI system. This phase establishes the "language" of the UI.
*   **Steps:**
    1.  **Create `IUiCompositionService.cs`**: Define the main service interface with methods like `RequestNavButton`, `RequestContainer`, `AddElementsToContainer`, `UpdateElement`, `RemoveElement`.
    2.  **Define UI Element Models**: Create C# records/classes for each UI component (`NavigationButton`, `ContentContainer`, `Card`, `Label`, `Button`, `TextInput`). These models will contain properties like `Id`, `OwnerId`, `ParentId`, `Content`, `Style`, `Permissions`.
    3.  **Define Request/Response Models**: Create models for requesting UI changes (e.g., `CreateNavButtonRequest`, `RenderElementsRequest`, `UpdateElementStyleRequest`).
    4.  **Define Ownership & Permissions Model**: Create a class (`ElementPermissions`) to encapsulate the complex ownership and sharing rules.

## Phase 2: The In-Memory Composition Engine

*   **Objective:** Implement the server-side logic that manages the state of the UI.
*   **Steps:**
    1.  **Implement `UiCompositionService.cs`**: This class will implement `IUiCompositionService`.
    2.  **UI State Management**: Implement a thread-safe dictionary or tree structure to hold the live model of the UI.
    3.  **Lifecycle Management**: Implement the logic for handling create, update, and delete requests. This includes generating unique IDs and enforcing the ownership and permission rules defined in Phase 1.

## Phase 3: Real-time Communication Layer (SignalR)

*   **Objective:** Establish a real-time, bidirectional communication channel between the backend engine and the browser client.
*   **Steps:**
    1.  **Create `UiHub.cs`**: Define a SignalR hub for UI communication.
    2.  **Integrate Service and Hub**: When the `UiCompositionService` processes a change, it will notify the `UiHub`.
    3.  **Define Client-side API**: Define the methods the hub will call on the client (e.g., `ElementAdded(elementModel)`, `ElementRemoved(elementId)`, `ElementUpdated(elementModel)`).
    4.  **Define Server-side API**: Define the methods the client can invoke on the hub (e.g., `OnElementClicked(string elementId, clickEventArgs)`).

## Phase 4: Host Integration & Frontend Scaffolding

*   **Objective:** Configure the Host to serve the UI and establish the initial frontend framework.
*   **Steps:**
    1.  **Service Registration (in `Host`)**: Register `IUiCompositionService`, `UiCompositionService`, and SignalR in the `Host` project's dependency injection container.
    2.  **Endpoint Mapping (in `Host`)**: Map the `UiHub` to an endpoint (e.g., `/ui-hub`).
    3.  **Create `index.html` (in `Host/wwwroot`)**: Create a minimal HTML shell with a root `<div>` for the app and the necessary `<script>` tags.
    4.  **Create `site.js` (in `Host/wwwroot/js`)**: This file will contain the client-side SignalR connection logic.
    5.  **Establish Connection**: Write the JavaScript code in `site.js` to connect to the `/ui-hub` endpoint when the page loads.

## Phase 5: Client-Side Rendering Engine

*   **Objective:** Write the JavaScript code that receives UI data from the backend and renders it as HTML DOM elements.
*   **Steps:**
    1.  **Implement SignalR Handlers**: In `site.js`, implement the client-side methods defined in Phase 3 (e.g., `connection.on("ElementAdded", ...)`).
    2.  **Create Dynamic Rendering Functions**: Write a generic `renderElement(elementModel)` JavaScript function. This function will inspect the incoming model and delegate to specific functions like `renderCard(model)`, `renderButton(model)`, etc.
    3.  **Implement Event Listeners**: Write logic to attach browser event listeners (e.g., `onclick`) to the rendered elements. These listeners will invoke methods on the `UiHub` to send user interactions back to the server.
