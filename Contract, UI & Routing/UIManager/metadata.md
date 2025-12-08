# Project: SoftwareCenter.UIManager

## Purpose
`SoftwareCenter.UIManager` (formerly UI.Engine) is the sole authority for all UI-related operations. It manages the state of the UI, processes requests to change the UI, and ensures that all UI elements are tracked and addressable. No other component manipulates the UI directly.

## Key Responsibilities
- **UI Command Processing:** Implement handlers for UI-specific commands (e.g., `CreateCardCommand`, `AddControlToContainerCommand`).
- **Element Management:** Generate and maintain a unique identifier for every UI element created (windows, containers, cards, buttons, etc.).
- **State Management:** Keep track of the current state of the UI, including which module owns which element.
- **Layout Definition:** Accept a base layout definition from the Host and populate it based on requests from modules.
- **Ownership:** Track ownership of UI elements, allowing owners to modify them or share ownership with other modules.

## Architectural Principles
- **Command-Driven:** All UI modifications are initiated by sending a command to the Kernel, which routes it to the UIManager.
- **No Direct Rendering:** The UIManager is framework-agnostic. It manages a model of the UI. The actual rendering is handled by the Host's frontend technology (e.g., Blazor) which binds to the state exposed by the UIManager.
- **Decoupled from Logic:** The UIManager does not know *why* a button is being added; it only knows how to add a button when it receives the appropriate command.
- **Referenced by Host:** The Host application initializes and holds a reference to the UIManager.

## Project Structure
```
SoftwareCenter.UIManager/
├── Commands/
│   ├── CreateCardCommand.cs
│   └── AddControlToContainerCommand.cs
├── Handlers/
│   ├── CreateCardCommandHandler.cs
│   └── AddControlToContainerCommandHandler.cs
├── Services/
│   └── UIStateService.cs
├── UIManagerServiceCollectionExtensions.cs
├── SoftwareCenter.UIManager.csproj
└── METADATA.md
```

## Dependencies
- `SoftwareCenter.Core`