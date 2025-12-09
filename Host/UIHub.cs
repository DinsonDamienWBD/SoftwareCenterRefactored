using Microsoft.AspNetCore.SignalR;

namespace SoftwareCenter.Host
{
    /// <summary>
    /// An empty SignalR Hub. All interaction is done via the IUIHubNotifier service
    /// which uses the IHubContext for this hub.
    /// </summary>
    public class UIHub : Hub
    {
    }
}
