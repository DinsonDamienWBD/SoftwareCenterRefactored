using SoftwareCenter.Core.Data.UI;
using SoftwareCenter.Core.Routing;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SoftwareCenter.UIManager.Services
{
    /// <summary>
    /// Manages the in-memory state of all UI elements. This service is the single source of truth for the UI's structure.
    /// It handles the registration, retrieval, and removal of UI elements, and manages priority-based overrides.
    /// </summary>
    public class UIStateService
    {
        // Main dictionary storing all registered UI fragments, keyed by their unique ID.
        private readonly ConcurrentDictionary<string, UIFragment> _fragments = new();

        // A lookup to track which fragments are associated with a given "slot" or parent.
        // This allows for efficient priority-based lookups. The string key is a conceptual "slot" name.
        private readonly ConcurrentDictionary<string, SortedSet<UIFragment>> _fragmentSlots = new();


        public bool TryAddFragment(UIFragment fragment)
        {
            if (!_fragments.TryAdd(fragment.Id, fragment))
            {
                return false;
            }

            // If the fragment has a slot name, add it to the priority-sorted set for that slot.
            if (!string.IsNullOrEmpty(fragment.SlotName))
            {
                var slot = _fragmentSlots.GetOrAdd(fragment.SlotName, _ => new SortedSet<UIFragment>(new FragmentPriorityComparer()));
                slot.Add(fragment);
            }
            return true;
        }

        public UIFragment GetFragment(string id)
        {
            _fragments.TryGetValue(id, out var fragment);
            return fragment;
        }

        public bool TryRemoveFragment(string id)
        {
            if (!_fragments.TryRemove(id, out var fragment))
            {
                return false;
            }

            // Also remove from the priority slot if it exists there.
            if (!string.IsNullOrEmpty(fragment.SlotName) && _fragmentSlots.TryGetValue(fragment.SlotName, out var slot))
            {
                slot.Remove(fragment);
            }
            return true;
        }

        /// <summary>
        /// Gets the highest-priority, active fragment for a given slot name.
        /// </summary>
        /// <param name="slotName">The name of the slot (e.g., "SourceManagement.MainView").</param>
        /// <returns>The highest priority UIFragment, or null if none are found.</returns>
        public UIFragment GetActiveFragmentForSlot(string slotName)
        {
            if (_fragmentSlots.TryGetValue(slotName, out var slot))
            {
                // The SortedSet ensures the first element is the one with the highest priority.
                return slot.FirstOrDefault();
            }
            return null;
        }

        public IEnumerable<UIFragment> GetAllFragments()
        {
            return _fragments.Values;
        }
    }

    /// <summary>
    /// Comparer to sort UIFragments by priority, with higher priority values coming first.
    /// </summary>
    file class FragmentPriorityComparer : IComparer<UIFragment>
    {
        public int Compare(UIFragment x, UIFragment y)
        {
            if (x == null || y == null) return 0;

            // Higher priority enum values should come first.
            int priorityComparison = y.Priority.CompareTo(x.Priority);
            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            // As a tie-breaker, use the ID.
            return string.Compare(x.Id, y.Id, System.StringComparison.Ordinal);
        }
    }
}
