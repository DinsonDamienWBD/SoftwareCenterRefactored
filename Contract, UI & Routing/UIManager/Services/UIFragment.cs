using SoftwareCenter.Core.Data;
using SoftwareCenter.Core.Routing;
using SoftwareCenter.Core.UI;

namespace SoftwareCenter.UIManager.Services
{
    /// <summary>
    /// Represents a piece of UI content registered with the UIManager.
    /// </summary>
    public class UIFragment
    {
        public string Id { get; set; }
        public string OwnerId { get; set; }
        public HandlerPriority Priority { get; set; }
        public string HtmlContent { get; set; }

        /// <summary>
        /// An optional name for a conceptual "slot" this fragment occupies.
        /// Allows for priority-based overriding of the same UI area.
        /// e.g., "SourceManagement.MainView"
        /// </summary>
        public string SlotName { get; set; }
        
        // Retain UIElement for more granular, non-fragment elements if needed in the future.
        // For now, we focus on the fragment-based registration.
        public UIElement Element { get; set; }
        
        public Dictionary<string, string> Attributes { get; set; } = new();    }
}
