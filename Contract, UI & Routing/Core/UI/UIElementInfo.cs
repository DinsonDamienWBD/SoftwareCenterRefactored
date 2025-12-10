using System.Collections.Generic;

namespace SoftwareCenter.Core.UI
{
    /// <summary>
    /// A Data Transfer Object (DTO) representing the serializable state of a UI element.
    /// This is used for communication with the frontend.
    /// </summary>
    public class UIElementInfo
    {
        /// <summary>
        /// Gets or sets the unique ID of the element.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the type of the element.
        /// </summary>
        public string ElementType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the module that owns the element.
        /// </summary>
        public string OwnerModuleId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the parent element.
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of properties for the element.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of child element IDs.
        /// </summary>
        public List<string> ChildrenIds { get; set; } = new();

        /// <summary>
        /// Gets or sets the rendering priority of the element.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the name of the slot this element occupies in its parent.
        /// </summary>
        public string? SlotName { get; set; }
    }
}
