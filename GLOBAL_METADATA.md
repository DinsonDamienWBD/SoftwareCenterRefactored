# Software Center - Global Architecture Metadata

## Overview

This document outlines the high-level architecture of the Software Center application. The architecture is designed as a decoupled, extensible, module-first platform. It is built around a central **Kernel** that acts as a service broker and communication bus. The **Host** application provides the entry point and default functionalities, which can be extended or replaced by **Modules**.

## Core Principles

1.  **Decoupling:** Components (Host, Modules) should not reference each other directly. All communication flows through the Kernel via commands, events, or API calls.
2.  **Dynamic Discovery:** The Kernel discovers services, commands, and APIs at runtime. Modules register their capabilities, and other components can query the Kernel to discover and use them without compile-time dependencies.
3.  **Host as Default Provider:** The Host application is a fully functional, standalone application that provides basic implementations for all core features (e.g., logging, installation).
4.  **Modules as Specialists:** Modules provide advanced or alternative implementations of features. They can replace the Host's default providers.
5.  **Single UI Authority:** The `UIManager` is the only component that can manipulate the user interface. All UI changes are requested via commands sent to the `UIManager`.
6.  **Minimal Core Contract:** The `Core` library contains only the essential, stable interfaces and models required to participate in the ecosystem (`IModule`, `ICommand`, etc.). It is the only project Modules are required to reference.

## Project Dependencies

The fundamental dependency graph is as follows:

```
              +-----------------+
              |      Host       |
              +--------+--------+
                       |
           +-----------+-----------+
           |                       |
           v                       v
+-----------+-----------+ +---------+-----------+
|         Kernel        | |       UIManager     |
+-----------+-----------+ +---------+-----------+
           |                       |
           +-----------+-----------+
                       |
                       v
+-----------------------+-----------------------+
|                      Core                     |
+-----------------------------------------------+
           ^
           |
+-----------+-----------+
|        Modules        |
+-----------------------+
```