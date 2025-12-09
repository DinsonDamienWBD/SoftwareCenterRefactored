using System.Collections.Generic;

namespace SoftwareCenter.Core.Commands.UI
{
    public class CreateUIElementCommand : ICommand<string>
    {
        public string ParentId { get; }
        public string ElementType { get; }
        public Dictionary<string, object> InitialProperties { get; }

        public CreateUIElementCommand(string parentId, string elementType, Dictionary<string, object> initialProperties = null)
        {
            ParentId = parentId;
            ElementType = elementType;
            InitialProperties = initialProperties ?? new Dictionary<string, object>();
        }
    }
}
