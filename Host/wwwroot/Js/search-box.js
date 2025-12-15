"use strict";

// Simple search box wiring
(function () {
    const debounce = (fn, wait) => {
        let t = null;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), wait);
        };
    };

    function renderSuggestions(container, suggestions) {
        container.innerHTML = '';
        if (!Array.isArray(suggestions) || suggestions.length === 0) return;
        suggestions.forEach(item => {
            const text = (typeof item === 'object') ? (item.label ?? item.value ?? String(item)) : String(item);
            const value = (typeof item === 'object') ? (item.value ?? text) : text;
            const div = document.createElement('div');
            div.className = 'std-search-suggestion';
            div.textContent = text;
            div.dataset.value = value;
            div.addEventListener('click', (e) => {
                const box = container.closest('.std-search-box');
                const input = box.querySelector('.std-search-input');
                input.value = value;
                container.hidden = true;
                dispatchSearch(box, value);
            });
            container.appendChild(div);
        });
    }

    function dispatchSearch(box, query) {
        const evt = new CustomEvent('ui:search', { detail: { query }, bubbles: true });
        box.dispatchEvent(evt);
    }

    function initBox(box) {
        if (box._searchBoxInit) return;
        box._searchBoxInit = true;

        const input = box.querySelector('.std-search-input');
        const btn = box.querySelector('.std-search-btn');
        const mode = box.querySelector('.std-search-mode');
        const dropdown = box.querySelector('.std-search-dropdown');

        const showDropdown = (arr) => {
            if (!dropdown) return;
            renderSuggestions(dropdown, arr);
            dropdown.hidden = false;
        };

        const hideDropdown = () => {
            if (!dropdown) return;
            dropdown.hidden = true;
        };

        const doSearch = (q) => {
            // If element has inline data-suggestions (JSON string), show them
            try {
                const ds = box.dataset.suggestions;
                if (ds) {
                    const parsed = JSON.parse(ds);
                    if (Array.isArray(parsed) && parsed.length) {
                        showDropdown(parsed);
                    }
                }
            } catch (e) {
                // ignore parse errors
            }

            dispatchSearch(box, q);
        };

        const debounced = debounce((ev) => {
            if (mode && mode.value === 'as-you-type') {
                doSearch(ev.target.value);
            }
        }, 300);

        if (input) {
            input.addEventListener('input', debounced);
            input.addEventListener('keydown', (ev) => {
                if (ev.key === 'Enter') {
                    if (!mode || mode.value === 'on-enter') {
                        ev.preventDefault();
                        doSearch(input.value);
                        hideDropdown();
                    }
                }
                if (ev.key === 'ArrowDown' && dropdown && !dropdown.hidden) {
                    const first = dropdown.querySelector('.std-search-suggestion');
                    if (first) { first.focus(); }
                }
            });
        }

        if (btn) {
            btn.addEventListener('click', () => {
                const q = input ? input.value : '';
                doSearch(q);
                hideDropdown();
            });
        }

        // Click outside hides dropdown
        document.addEventListener('click', (e) => {
            if (!box.contains(e.target)) hideDropdown();
        });

        // Provide programmatic API
        box.setSuggestions = (arr) => {
            box.dataset.suggestions = JSON.stringify(arr);
            const dropdownArr = arr || [];
            if (dropdownArr.length) renderSuggestions(dropdown, dropdownArr);
        };
    }

    function initAll() {
        document.querySelectorAll('.std-search-box').forEach(initBox);
    }

    // Auto-init on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initAll);
    } else {
        initAll();
    }

    // Expose helper
    window.SearchBox = {
        attach: (el) => initBox(typeof el === 'string' ? document.querySelector(el) : el),
        setSuggestions: (el, arr) => {
            const box = typeof el === 'string' ? document.querySelector(el) : el;
            if (!box) return;
            box.setSuggestions(arr);
        }
    };

})();
