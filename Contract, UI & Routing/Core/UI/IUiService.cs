using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareCenter.Core.UI
{
    public interface IUiService
    {
        // Called by Host MainController to get the initial index.html
        Task<string> GetComposedIndexPageAsync();

        // Called by Modules (via API/SignalR) to render a manifest into HTML
        // Returns: The fully built HTML string with GUIDs assigned
        Task<string> RenderManifestAsync(UiManifest manifest);

        // Helper to get just a single component's HTML (for internal use)
        Task<string> BuildComponentHtmlAsync(ComponentDefinition component);
    }
}
