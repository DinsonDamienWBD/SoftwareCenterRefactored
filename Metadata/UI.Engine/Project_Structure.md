# UI.Engine Project Structure

This document outlines the key components and namespaces within the `SoftwareCenter.UI.Engine` project.

## Namespaces

### `SoftwareCenter.UI.Engine`
- **UICompositionEngine.cs**: The primary concrete implementation of the `IUIEngine` interface. This class is the heart of the engine.
    - **Responsibilities:**
        - Implements all methods from `IUIEngine`.
        - Manages the internal state of the UI tree.
        - Invokes an external callback to notify the `Host` of UI changes.

### `SoftwareCenter.UI.Engine.Internal`
- This namespace will contain the internal data models that represent the live UI elements. These are the concrete objects stored in the engine's state dictionaries.
- **InternalUIElement.cs**: A base class for internal element representations.
- **InternalState.cs**: A class to hold all the `ConcurrentDictionary` collections that represent the complete UI tree.
- **UIChange.cs**: A data object used to describe a change to the UI (e.g., `ElementAdded`, `PropertyUpdated`) that gets sent to the `Host`'s communication bridge.
