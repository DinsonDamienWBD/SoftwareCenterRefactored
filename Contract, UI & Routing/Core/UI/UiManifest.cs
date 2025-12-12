using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareCenter.Core.UI
{
    // The envelope sent by Modules to request UI changes
    public class UiManifest
    {
        public string ModuleName { get; set; }

        // "inject", "update", "remove"
        public string Operation { get; set; } = "inject";

        // The GUID of the container we are targeting
        public string TargetGuid { get; set; }

        // The specific slot inside the target (default="default")
        public string MountPoint { get; set; } = "default";

        // The root element to build and inject
        public ComponentDefinition RootComponent { get; set; }
    }

    // The recursive structure defining the UI tree
    public class ComponentDefinition
    {
        // "tpl-card", "tpl-button" OR "custom"
        public string Type { get; set; }

        // Text content to inject into {{CONTENT}}
        public string Content { get; set; }

        // HTML Attributes (e.g. { "class": "highlight", "src": "..." })
        public Dictionary<string, string> Attributes { get; set; }

        // Recursive children
        public List<ComponentDefinition> Children { get; set; }

        // RAW HTML (Only used if Type == "custom")
        public string RawHtml { get; set; }

        // CSS (Only used if Type == "custom")
        public string RawCss { get; set; }
    }
}
