using Microsoft.AspNetCore.SignalR;
using SoftwareCenter.Core.UI;
using System.Threading.Tasks;

namespace SoftwareCenter.Host.Hubs
{
    public class UiHub : Hub
    {
        private readonly IUiService _uiService;

        public UiHub(IUiService uiService)
        {
            _uiService = uiService;
        }

        // Called by Modules (via a local client or API bridge) to inject UI
        public async Task ProcessManifest(UiManifest manifest)
        {
            // 1. Build the HTML on the server
            var htmlOutput = await _uiService.RenderManifestAsync(manifest);

            // 2. Determine Payload for Client
            // Corresponds to 'addFragment' payload in app.js
            var clientPayload = new
            {
                targetGuid = manifest.TargetGuid,
                mountPoint = manifest.MountPoint,
                htmlContent = htmlOutput,
                // If it's a custom element with CSS, we might send it separately, 
                // but for now let's assume rawCss is handled by a specific 'InjectStyle' event if needed.
                newGuid = "EXTRACTED_FROM_HTML" // Client logic handles finding the guid in the HTML
            };

            // 3. Send to Frontend
            if (manifest.Operation == "inject")
            {
                await Clients.All.SendAsync("InjectFragment", clientPayload);
            }
            else if (manifest.Operation == "update")
            {
                await Clients.All.SendAsync("UpdateFragment", clientPayload);
            }
            else if (manifest.Operation == "remove")
            {
                await Clients.All.SendAsync("RemoveFragment", new { targetGuid = manifest.TargetGuid });
            }
        }
    }
}