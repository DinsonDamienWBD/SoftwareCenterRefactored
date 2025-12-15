"use strict";

// ---
// CLIENT-SIDE RENDERER (Host)
// Responsibilities:
// 1. Establish SignalR connection.
// 2. Listen for 'Inject', 'Update', 'Remove' commands from UIManager.
// 3. Locate target containers via data-GUID and data-mount-point.
// ---

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/uihub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// --- DOM Manipulation Factory ---

const uiRenderer = {
    /**
     * Converts an HTML string into a DOM element.
     */
    createElementFromHtml: (htmlString) => {
        const template = document.createElement('template');
        template.innerHTML = htmlString.trim();
        return template.content.firstChild;
    },

    /**
     * Locates a component by its GUID and an optional internal mount point.
     * @param {string} targetGuid - The GUID of the parent component.
     * @param {string} mountPointName - The name of the specific slot (default: 'default').
     */
    findMountPoint: (targetGuid, mountPointName = 'default') => {
        // 1. Find the parent component shell
        const parentComponent = document.querySelector(`[data-GUID='${targetGuid}']`);
        if (!parentComponent) return null;

        // 2. Look for the specific mount point inside it
        let mountNode = parentComponent.querySelector(`[data-mount-point='${mountPointName}']`);

        // 3. Fallback: If the parent ITSELF is the mount point (common in simple zones)
        if (!mountNode && parentComponent.getAttribute('data-mount-point') === mountPointName) {
            mountNode = parentComponent;
        }

        return mountNode;
    },

    /**
     * Injects a new fragment into the UI.
     * Payload matches the standard we discussed.
     */
    addFragment: (payload) => {
        // Payload: { targetGuid, mountPoint, htmlContent, newGuid }

        const mountNode = uiRenderer.findMountPoint(payload.targetGuid, payload.mountPoint);

        if (!mountNode) {
            console.error(`Target GUID '${payload.targetGuid}' with mount-point '${payload.mountPoint}' not found.`);
            return;
        }

        // Create the element
        const newElement = uiRenderer.createElementFromHtml(payload.htmlContent);

        // Ensure the new element has the assigned GUID (if not already in the HTML)
        if (!newElement.getAttribute('data-GUID')) {
            newElement.setAttribute('data-GUID', payload.newGuid);
        }

        // Append to the DOM
        mountNode.appendChild(newElement);

        console.log(`Fragment ${payload.newGuid} injected into ${payload.targetGuid} @ ${payload.mountPoint}`);

        // Auto-initialize composite controls (e.g. search-box) so UIManager ownership is wired
        try {
            const autoInit = async () => {
                if (!window.UIManagerClient) return; // helper not available

                // Helper to derive moduleId from payload or element dataset
                const deriveModuleId = (el) => {
                    return payload && (payload.requestingModuleId || payload.moduleId || payload.sourceModule || payload.ownerModule)
                        || (el && (el.dataset.requestingModule || el.dataset.moduleId))
                        || null;
                };

                const initBox = async (box) => {
                    const moduleId = deriveModuleId(box);
                    try {
                        await window.UIManagerClient.mountSearchBox(box, moduleId, { autoForward: true });
                        console.log('Mounted search box for', moduleId, box);
                    } catch (e) {
                        console.warn('Failed to auto-mount search box', e);
                    }
                };

                // If the injected element itself is a search box
                if (newElement.classList && newElement.classList.contains('std-search-box')) {
                    await initBox(newElement);
                }

                // Any nested search-box elements
                const nested = newElement.querySelectorAll ? newElement.querySelectorAll('.std-search-box') : [];
                for (const el of nested) {
                    await initBox(el);
                }
            };

            // schedule async init without blocking the main flow
            setTimeout(() => autoInit().catch(() => {}), 0);
        } catch (e) {
            console.warn('Auto-init controls failed', e);
        }
    },

    /**
     * Updates an existing fragment.
     */
    updateFragment: (payload) => {
        // Payload: { targetGuid, htmlContent (optional), attributes (optional) }
        const element = document.querySelector(`[data-GUID='${payload.targetGuid}']`);
        if (!element) return;

        if (payload.htmlContent) {
            // CAUTION: This wipes internal state/listeners. Use carefully.
            element.innerHTML = payload.htmlContent;
        }

        if (payload.attributes) {
            for (const [key, value] of Object.entries(payload.attributes)) {
                if (value === null) element.removeAttribute(key);
                else element.setAttribute(key, value);
            }
        }
    },

    /**
     * Removes a fragment.
     */
    removeFragment: (payload) => {
        // Payload: { targetGuid }
        const element = document.querySelector(`[data-GUID='${payload.targetGuid}']`);
        if (element) element.remove();
    }
};

// Expose connection and renderer so UIManager helpers can forward events or use renderer APIs.
window.uiHubConnection = connection;
window.uiRenderer = uiRenderer;


// --- SignalR Event Listeners ---

// NOTE: These event names should match what your UIManager (Backend) sends.
connection.on("InjectFragment", (payload) => uiRenderer.addFragment(payload));
connection.on("UpdateFragment", (payload) => uiRenderer.updateFragment(payload));
connection.on("RemoveFragment", (payload) => uiRenderer.removeFragment(payload));

// Fallback listener: if UIManagerClient falls back to emitting 'uimanager:route', forward to hub.
document.addEventListener('uimanager:route', async (e) => {
    try {
        const detail = e && e.detail ? e.detail : null;
        if (!detail) return;
        const { moduleId, commandName, payload } = detail;
        if (connection && typeof connection.invoke === 'function') {
            // Try to route to module via hub. Host-side hub should expose a RouteToModule method.
            await connection.invoke('RouteToModule', moduleId, commandName, payload);
            console.log('Routed uimanager:route to hub for', moduleId, commandName);
        } else {
            console.warn('SignalR connection not available to route uimanager:route', detail);
        }
    } catch (err) {
        console.warn('Error forwarding uimanager:route to hub', err);
    }
});


// --- Host Interaction Logic ---
// Basic interactions for the static elements provided by the Host Zones
// (Notification toggle, Power dropdown, etc.)

document.addEventListener('DOMContentLoaded', () => {
    // Note: We DO NOT fetch HTML here anymore. 
    // The server injects the initial zones before sending index.html.

    // Helper for dropdowns
    const setupDropdown = (triggerId, dropdownId) => {
        const trigger = document.getElementById(triggerId);
        const dropdown = document.getElementById(dropdownId);

        if (trigger && dropdown) {
            trigger.addEventListener('click', (e) => {
                e.stopPropagation();
                const isVisible = dropdown.style.display === 'block';
                // Close all others first (optional simple logic)
                document.querySelectorAll('.flyout, .dropdown').forEach(el => el.style.display = 'none');
                // Toggle current
                dropdown.style.display = isVisible ? 'none' : 'block';
            });
        }
    };

    setupDropdown('notification-zone', 'notification-flyout');
    setupDropdown('power-zone', 'power-dropdown');

    // Global click to close dropdowns
    document.addEventListener('click', () => {
        document.querySelectorAll('.flyout, .dropdown').forEach(el => el.style.display = 'none');
    });
});

// --- Start Connection ---
async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setTimeout(start, 5000);
    }
};

start();