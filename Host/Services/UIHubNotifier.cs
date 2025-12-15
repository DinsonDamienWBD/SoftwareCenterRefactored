using Microsoft.AspNetCore.SignalR;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.Core.Events;
using SoftwareCenter.Core.Events.UI;
using SoftwareCenter.Core.UI;
using SoftwareCenter.UIManager.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SoftwareCenter.Host.Services
{
    /// <summary>
    /// Listens for UI-related events from the core event bus and notifies
    /// connected SignalR clients of the changes. This class acts as the
    /// bridge between the .NET backend and the JavaScript frontend.
    /// </summary>
    public class UIHubNotifier :
        IUIHubNotifier, // Implements the new interface
        IEventHandler<UIElementRegisteredEvent>,
        IEventHandler<UIElementUnregisteredEvent>,
        IEventHandler<UIElementUpdatedEvent>
    {
        private readonly IHubContext<UIHub> _hubContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UIHubNotifier"/> class.
        /// </summary>
        /// <param name="hubContext">The SignalR hub context.</param>
        public UIHubNotifier(IHubContext<UIHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // --- IUIHubNotifier Implementation ---

        public Task InjectFragment(string targetGuid, string mountPoint, string htmlContent)
        {
            var clientPayload = new
            {
                targetGuid,
                mountPoint,
                htmlContent,
                newGuid = "EXTRACTED_FROM_HTML" 
            };
            return _hubContext.Clients.All.SendAsync("InjectFragment", clientPayload);
        }

        public Task UpdateFragment(string targetGuid, string mountPoint, string htmlContent)
        {
            var clientPayload = new
            {
                targetGuid,
                mountPoint,
                htmlContent,
                newGuid = "EXTRACTED_FROM_HTML"
            };
            return _hubContext.Clients.All.SendAsync("UpdateFragment", clientPayload);
        }

        public Task RemoveFragment(string targetGuid)
        {
            return _hubContext.Clients.All.SendAsync("RemoveFragment", new { targetGuid });
        }


        // --- IEventHandler Implementations ---

        /// <summary>
        /// Handles the event for a new UI element being registered and sends it to clients.
        /// </summary>
        public Task Handle(UIElementRegisteredEvent anEvent, ITraceContext traceContext)
        {
            var payload = new UIElementInfo
            {
                Id = anEvent.NewElement.Id,
                ParentId = anEvent.NewElement.ParentId,
                ElementType = anEvent.NewElement.ElementType.ToString(),
                OwnerModuleId = anEvent.NewElement.OwnerModuleId,
                Priority = anEvent.NewElement.Priority,
                SlotName = anEvent.NewElement.SlotName,
                Properties = anEvent.NewElement.Properties,
                ChildrenIds = anEvent.NewElement.Children.Select(c => c.Id).ToList()
            };

            // Add the raw content to the properties dictionary for the client
            payload.Properties["htmlContent"] = anEvent.HtmlContent;
            payload.Properties["cssContent"] = anEvent.CssContent;
            payload.Properties["jsContent"] = anEvent.JsContent;
            
            return _hubContext.Clients.All.SendAsync("ElementAdded", payload);
        }

        /// <summary>
        /// Handles the event for a UI element being removed and sends it to clients.
        /// </summary>
        public Task Handle(UIElementUnregisteredEvent anEvent, ITraceContext traceContext)
        {
            // Only the ID is needed to remove an element on the client
            var payload = new UIElementInfo { Id = anEvent.ElementId };
            return _hubContext.Clients.All.SendAsync("ElementRemoved", payload);
        }

        /// <summary>
        /// Handles the event for a UI element being updated and sends it to clients.
        /// </summary>
        public Task Handle(UIElementUpdatedEvent anEvent, ITraceContext traceContext)
        {
            var payload = new UIElementInfo
            {
                Id = anEvent.ElementId,
                Properties = anEvent.UpdatedProperties
            };
            return _hubContext.Clients.All.SendAsync("ElementUpdated", payload);
        }
    }
}
