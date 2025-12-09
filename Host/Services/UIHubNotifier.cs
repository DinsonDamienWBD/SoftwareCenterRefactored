using Microsoft.AspNetCore.SignalR;
using SoftwareCenter.UIManager.Services;
using System.Threading.Tasks;

namespace SoftwareCenter.Host.Services
{
    public class UIHubNotifier : IUIHubNotifier
    {
        private readonly IHubContext<UIHub> _hubContext;

        public UIHubNotifier(IHubContext<UIHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task ElementAdded(object elementData)
        {
            return _hubContext.Clients.All.SendAsync("ElementAdded", elementData);
        }

        public Task ElementRemoved(object elementData)
        {
            return _hubContext.Clients.All.SendAsync("ElementRemoved", elementData);
        }

        public Task ElementUpdated(object elementData)
        {
            return _hubContext.Clients.All.SendAsync("ElementUpdated", elementData);
        }
    }
}
