# Implementation Plan: SoftwareCenter.UI.Engine

This document outlines the phased development plan for the UI.Engine project.

## Phase 1: Core Contracts & Engine Foundation

**Goal:** Establish the fundamental communication channel between UI requestors (modules, Host) and the UI Engine, and create the engine's basic state management.

- **Step 1.1: Define UI Request Contracts in `SoftwareCenter.Core`**:
    - Create commands for all basic UI operations (`RequestNavButtonCommand`, `RequestContainerCommand`, `RenderUIFragmentCommand`, `UpdateUIFragmentCommand`, `DisposeUIFragmentCommand`).
    - Define interfaces/records for UI element metadata (`UIElementDescriptor`, etc.) that will be part of the command results.
- **Step 1.2: Create the `UICompositionEngine` Service**:
    - This central service in `UI.Engine` will manage the application's complete UI state in memory.
    - It will use a `Dictionary<Guid, UIElement>` to track every UI element, its properties, its owner, and its permissions.
- **Step 1.3: Implement Command Handlers**:
    - Create handlers in `UI.Engine` for each UI command defined in `Core`.
    - Initially, these handlers will validate the request and update the in-memory UI state in the `UICompositionEngine`.

## Phase 2: Web Communication & Basic Rendering

**Goal:** Transmit the UI state from the C# backend to the browser frontend for rendering.

- **Step 2.1: Implement a WebSocket Hub (using SignalR)**:
    - The `UI.Engine` will contain a SignalR hub definition for real-time UI change communication.
    - The `Host` project will be responsible for mapping the endpoint for this hub during application startup.
- **Step 2.2: Define a Frontend Communication Protocol**:
    - Specify a clear JSON-based message format for UI actions (e.g., `{ action: 'add', element: { ... } }`, `{ action: 'remove', elementId: '...' }`).
- **Step 2.3: Connect Engine to Hub**:
    - When a command handler updates the UI state in the `UICompositionEngine`, the engine will trigger an event. A listener service will convert this state change into a protocol message and broadcast it via the SignalR hub to connected clients.
- **Step 2.4: Create a Basic Frontend Client**:
    - Develop a simple `main.js` in `Host/wwwroot` that connects to the SignalR hub. This script will listen for messages and perform basic DOM manipulation to render, update, and remove elements, dynamically building the UI based on backend commands.

## Phase 3: Advanced Rendering & Ownership Model

**Goal:** Implement complex UI features and the security/ownership model.

- **Step 3.1: Implement Advanced Rendering Logic**:
    - Enhance the `RenderUIFragmentCommand` handler to support the different rendering modes: rendering an array of pre-defined controls, creating card-based layouts, and safely injecting custom HTML/CSS/JS (using an HTML sanitizer to prevent XSS).
- **Step 3.2: Expand the UI Element Data Model**:
    - Add `OwnerId` (e.g., a Module ID) and `Permissions` properties to the `UIElement` class stored in the `UICompositionEngine`.
- **Step 3.3: Define Ownership Commands in `SoftwareCenter.Core`**:
    - Create commands for managing UI ownership and permissions (`TransferOwnershipCommand`, `SetPermissionsCommand`, `ShareOwnershipCommand`).
- **Step 3.4: Implement and Enforce Permissions**:
    - Implement the handlers in `UI.Engine` for the new ownership commands.
    - Critically, update *all* UI-modifying command handlers (`UpdateUIFragmentCommand`, `DisposeUIFragmentCommand`) to check the requestor's permissions against the element's stored permissions before executing any change.
