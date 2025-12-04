# Core Project Structure

This document outlines the key components, namespaces, and contracts within the `SoftwareCenter.Core` project.

## Namespaces

### `SoftwareCenter.Core.Commands`
- **ICommand.cs**: Marker interface for commands.
- **IResult.cs**: Interface for command results.
- **Result.cs**: A concrete implementation of `IResult`.

### `SoftwareCenter.Core.Events`
- **IEvent.cs**: Marker interface for domain events.

### `SoftwareCenter.Core.Modules`
- **IModule.cs**: The primary interface for all dynamically-loaded modules.

### `SoftwareCenter.Core.Kernel`
- **IKernel.cs**: The contract for the application kernel, defining methods for routing and module management.
- **New Additions (Phase 3):**
    - `void RegisterService<T>(T service);`
    - `T GetService<T>();`

### `SoftwareCenter.Core.UI` (New - Phase 3)
This new namespace will house all contracts related to the UI Engine.

- **IUIEngine.cs**: The main interface for the UI Composition Engine.
    - `Guid RequestNavigation(NavigationRequest request);`
    - `Guid CreateCard(Guid containerId, CardConfiguration config);`
    - `void RenderElements(Guid parentId, IEnumerable<UIElement> elements);`
    - `void UpdateElement(Guid elementId, ElementUpdatePayload payload);`
    - `void RemoveElement(Guid elementId);`

- **`SoftwareCenter.Core.UI.Contracts`** (Sub-namespace)
    - **NavigationRequest.cs**: Class for requesting top-level navigation.
    - **CardConfiguration.cs**: Class for defining a card's initial state.
    - **UIElement.cs**: Abstract base class for all UI controls.
    - **Button.cs, Label.cs, etc.**: Concrete `UIElement` implementations.
    - **ElementUpdatePayload.cs**: Class for packaging property updates for an element.