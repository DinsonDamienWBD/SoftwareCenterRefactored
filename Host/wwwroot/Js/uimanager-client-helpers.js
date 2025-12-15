"use strict";

// UIManager client helpers
// Responsibilities:
// - load Host-provided control scripts (like search-box.js) lazily
// - initialize mounted controls and wire events so UIManager can forward them to modules
// - provide a standard DOM event contract for routing when SignalR is unavailable

(function () {
    function loadScriptOnce(src) {
        return new Promise((resolve, reject) => {
            if (document.querySelector(`script[src="${src}"]`)) return resolve();
            const s = document.createElement('script');
            s.src = src;
            s.onload = () => resolve();
            s.onerror = (e) => reject(e);
            document.head.appendChild(s);
        });
    }

    async function ensureSearchBoxScript() {
        // Host serves this asset at /js/search-box.js
        const src = '/js/search-box.js';
        await loadScriptOnce(src);
        // Wait for the script to expose SearchBox
        if (!window.SearchBox) {
            // small nextTick wait
            await new Promise(r => setTimeout(r, 0));
        }
        return window.SearchBox;
    }

    // Forwarding logic: prefer SignalR if exposed as window.uiHubConnection with invoke/send
    async function forwardToModule(moduleId, commandName, payload) {
        const conn = window.uiHubConnection;
        // If SignalR connection exposes `invoke`, try to call a routing method on the hub
        if (conn && typeof conn.invoke === 'function') {
            try {
                // Hub method name 'RouteToModule' is a suggested convention — backend should implement it.
                await conn.invoke('RouteToModule', moduleId, commandName, payload);
                return { forwarded: true };
            } catch (e) {
                // fallthrough to event
                console.warn('uiHubConnection.invoke failed, falling back to DOM event', e);
            }
        }

        // Fallback: emit a DOM event that host-side UIManager code can listen to
        const evt = new CustomEvent('uimanager:route', { detail: { moduleId, commandName, payload }, bubbles: true });
        document.dispatchEvent(evt);
        return { forwarded: false };
    }

    // Mount a search box that was injected by UIManager (or Host) into the DOM.
    // - container: the element that contains the `.std-search-box` (or the box element itself)
    // - moduleId: identifier of requesting module (string)
    // - options: { autoForward: true|false } — if true, the helper will forward ui:search events automatically
    async function mountSearchBox(container, moduleId, options = {}) {
        const box = container.classList && container.classList.contains('std-search-box') ? container : container.querySelector('.std-search-box');
        if (!box) throw new Error('std-search-box not found in container');

        await ensureSearchBoxScript();

        // Attach control behavior
        if (window.SearchBox && typeof window.SearchBox.attach === 'function') {
            window.SearchBox.attach(box);
        }

        // Wire search events to forward to module
        const handler = async (e) => {
            const query = e && e.detail ? e.detail.query : '';
            const payload = { query, origin: box.getAttribute('data-GUID') || null };
            await forwardToModule(moduleId, 'SearchQuery', payload);
        };

        box.addEventListener('ui:search', handler);

        // Provide a small API on the element so UIManager or module code can call it directly
        box.uimanager = box.uimanager || {};
        box.uimanager.setSuggestions = (arr) => {
            if (window.SearchBox && typeof window.SearchBox.setSuggestions === 'function') {
                window.SearchBox.setSuggestions(box, arr);
            } else {
                // fallback: set data-suggestions attribute
                try {
                    box.dataset.suggestions = JSON.stringify(arr || []);
                } catch (e) {
                    box.removeAttribute('data-suggestions');
                }
            }
        };

        // Optionally auto-forward the searches (default true)
        if (options.autoForward !== false) {
            box.addEventListener('ui:search', handler);
        }

        return {
            box,
            detach: () => {
                box.removeEventListener('ui:search', handler);
            }
        };
    }

    // Expose helper API
    window.UIManagerClient = {
        loadScriptOnce,
        ensureSearchBoxScript,
        mountSearchBox,
        forwardToModule
    };

})();
