namespace SoftwareCenter.Core.UI
{
    // This class can be expanded with more properties as needed.
    public class UIElement
    {
        public string Id { get; set; }
        public string ElementType { get; set; }
        public string OwnerModuleId { get; set; }
        public string ParentId { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        public List<UIElement> Children { get; set; } = new List<UIElement>();
    }
}