using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Data.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.UIManager.Services;
using System;
using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Handlers
{
    public class RegisterUIFragmentCommandHandler : ICommandHandler<RegisterUIFragmentCommand, string>
    {
        private readonly UIStateService _uiStateService;
        private readonly IUIHubNotifier _hubNotifier;

        public RegisterUIFragmentCommandHandler(UIStateService uiStateService, IUIHubNotifier hubNotifier)
        {
            _uiStateService = uiStateService;
            _hubNotifier = hubNotifier;
        }

        public async Task<string> Handle(RegisterUIFragmentCommand command, TraceContext traceContext)
        {
            var elementId = $"el_{Guid.NewGuid().ToString("N")}";
            var ownerId = traceContext.ModuleId; // Assuming ModuleId identifies the owner

            var slotName = command.SlotName;
            
            var uiElement = new UIElement(elementId, ownerId, "Fragment");
            
            var fragment = new UIFragment
            {
                Id = elementId,
                OwnerId = ownerId,
                HtmlContent = command.HtmlContent,
                Priority = command.Priority,
                SlotName = slotName,
                Element = uiElement
            };
            
            if (_uiStateService.TryAddFragment(fragment))
            {
                var activeFragment = _uiStateService.GetActiveFragmentForSlot(slotName);
                
                // If the newly added fragment is the highest priority one, notify the client to add it.
                if(activeFragment?.Id == fragment.Id)
                {
                    await _hubNotifier.ElementAdded(new
                    {
                        ElementId = fragment.Id,
                        ParentId = command.ParentId, // The logical parent from the client's perspective
                        HtmlContent = fragment.HtmlContent,
                        CssContent = command.CssContent,
                        JsContent = command.JsContent
                    });
                }
                
                return elementId;
            }
            // Handle failure case
            return null; 
        }
    }
}
