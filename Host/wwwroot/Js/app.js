"use strict";

// ---
//
// This script is the client-side renderer for the UIManager.
// It connects to the UIHub via SignalR and performs DOM manipulations
// based on events received from the server. It also contains
// basic UI interaction logic for the main shell.
//
// ---

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/uihub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// --- Element Manipulation Functions ---

const elementFactory = {
    /**
     * Creates a new element from an HTML string.
     * @param {string} htmlString The HTML content.
     * @returns {HTMLElement} The created element.
     */
    createElement: (htmlString) => {
        const template = document.createElement('template');
        template.innerHTML = htmlString.trim();
        return template.content.firstChild;
    },

    /**
     * Adds an element to the DOM.
     * @param {object} elementData - The element data from the server.
     * @param {string} elementData.elementId - The ID of the element to add.
     * @param {string} elementData.parentId - The ID of the parent element.
     * @param {string} elementData.htmlContent - The inner HTML of the new element.
     * @param {string} [elementData.cssContent] - Optional CSS to inject.
     */
    addElement: (elementData) => {
        const parent = document.getElementById(elementData.parentId);
        if (!parent) {
            console.error(`Parent element with ID '${elementData.parentId}' not found.`);
            return;
        }

        // Avoid adding if it already exists
        if (document.getElementById(elementData.elementId)) {
            console.warn(`Element with ID '${elementData.elementId}' already exists.`);
            return;
        }

        const newElement = elementFactory.createElement(elementData.htmlContent);
        newElement.id = elementData.elementId;

        // Handle specific logic, e.g., for navigation
        if (parent.id === 'nav-rail-zone') {
            newElement.addEventListener('click', () => handleNavClick(newElement.id));
        }
        
        parent.appendChild(newElement);

        if (elementData.cssContent) {
            const style = document.createElement('style');
            style.textContent = elementData.cssContent;
            document.head.appendChild(style);
        }
    },

    /**
     * Updates an existing element.
     * @param {object} elementData - The element data from the server.
     * @param {string} elementData.elementId - The ID of the element to update.
     * @param {string} [elementData.htmlContent] - The new inner HTML.
     * @param {object} [elementData.attributes] - An object of attributes to set (e.g., {'class': 'new-class', 'data-value': '123'}).
     */
    updateElement: (elementData) => {
        const element = document.getElementById(elementData.elementId);
        if (!element) {
            console.error(`Element with ID '${elementData.elementId}' not found for update.`);
            return;
        }

        if (elementData.htmlContent) {
            element.innerHTML = elementData.htmlContent;
        }

        if (elementData.attributes) {
            for (const [key, value] of Object.entries(elementData.attributes)) {
                if (value === null) {
                    element.removeAttribute(key);
                } else {
                    element.setAttribute(key, value);
                }
            }
        }
    },

    /**
     * Removes an element from the DOM.
     * @param {object} elementData - The element data from the server.
     * @param {string} elementData.elementId - The ID of the element to remove.
     */
    removeElement: (elementData) => {
        const element = document.getElementById(elementData.elementId);
        if (element) {
            element.parentNode.removeChild(element);
        }
    } // Removed 'else' block for brevity, assuming it's not critical for the diff
};

// --- Event Handlers ---

/**
 * Handles clicks on navigation buttons.
 * @param {string} navButtonId - The ID of the clicked nav button.
 */
function handleNavClick(navButtonId) {
    // Deactivate all content containers and nav buttons
    document.querySelectorAll('#content-zone .content-container').forEach(c => c.classList.remove('active'));
    document.querySelectorAll('#nav-rail-zone .nav-button').forEach(b => b.classList.remove('active'));
    
    const navButton = document.getElementById(navButtonId);
    if (!navButton) {
        console.error(`Nav button with ID '${navButtonId}' not found.`);
        return;
    }

    // Activate the target content container and nav button
    const contentId = navButton.dataset.targetContainer; // Use the data attribute
    const contentContainer = document.getElementById(contentId);

    if (contentContainer) {
        contentContainer.classList.add('active');
    } else {
        console.error(`Content container with ID '${contentId}' not found.`);
    }

    navButton.classList.add('active');
}

/**
 * Fetches HTML content from a given path and inserts it into the element specified by elementId.
 * @param {string} elementId - The ID of the element to insert content into.
 * @param {string} htmlFilePath - The path to the HTML file to fetch.
 * @returns {Promise<void>} A promise that resolves when the HTML is loaded.
 */
async function loadHtmlIntoElement(elementId, htmlFilePath) {
    try {
        const response = await fetch(htmlFilePath);
        if (!response.ok) {
            throw new Error(`Failed to fetch ${htmlFilePath}: ${response.statusText}`);
        }
        const html = await response.text();
        const element = document.getElementById(elementId);
        if (element) {
            element.innerHTML = html;
        } else {
            console.error(`Element with ID '${elementId}' not found for HTML injection.`);
        }
    } catch (error) {
        console.error(`Error loading HTML into '${elementId}':`, error);
    }
}


// --- UI Interaction Logic ---

document.addEventListener('DOMContentLoaded', async () => {
    // Load all zone HTML content
    await loadHtmlIntoElement('titlebar-zone', '../Html/titlebar-zone.html');
    await loadHtmlIntoElement('nav-rail-zone', '../Html/nav-rail-zone.html');
    await loadHtmlIntoElement('content-zone', '../Html/content-zone.html');

    // After all zones are loaded, attach event listeners for interactive elements
    const notificationIcon = document.getElementById('notification-zone');
    const notificationFlyout = document.getElementById('notification-flyout');
    const powerIcon = document.getElementById('power-zone');
    const powerDropdown = document.getElementById('power-dropdown');

    const toggleVisibility = (element) => {
        if (element.style.display === 'block') {
            element.style.display = 'none';
        } else {
            element.style.display = 'block';
        }
    };

    if (notificationIcon && notificationFlyout) {
        notificationIcon.addEventListener('click', (event) => {
            event.stopPropagation();
            toggleVisibility(notificationFlyout);
            if (powerDropdown) powerDropdown.style.display = 'none'; // Close other dropdowns
        });
    }

    if (powerIcon && powerDropdown) {
        powerIcon.addEventListener('click', (event) => {
            event.stopPropagation();
            toggleVisibility(powerDropdown);
            if (notificationFlyout) notificationFlyout.style.display = 'none'; // Close other flyouts
        });
    }

    // Close flyouts/dropdowns if clicking anywhere else on the page
    document.addEventListener('click', (event) => {
        if (notificationIcon && notificationFlyout && !notificationIcon.contains(event.target)) {
            notificationFlyout.style.display = 'none';
        }
        if (powerIcon && powerDropdown && !powerIcon.contains(event.target)) {
            powerDropdown.style.display = 'none';
        }
    });
});


// --- SignalR Event Listeners ---

connection.on("ElementAdded", (elementData) => {
    console.log("ElementAdded:", elementData);
    elementFactory.addElement(elementData);
});

connection.on("ElementUpdated", (elementData) => {
    console.log("ElementUpdated:", elementData);
    elementFactory.updateElement(elementData);
});

connection.on("ElementRemoved", (elementData) => {
    console.log("ElementRemoved:", elementData);
    elementFactory.removeElement(elementData);
});

// --- Connection Start ---

async function start() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.log(err);
        setTimeout(start, 5000);
    }
};

connection.onclose(async () => {
    await start();
});

// Start the connection
start();
