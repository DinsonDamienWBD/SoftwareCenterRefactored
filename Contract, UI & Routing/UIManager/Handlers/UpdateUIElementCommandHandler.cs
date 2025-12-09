using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.UIManager.Services;
using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Handlers
{
    public class UpdateUIElementCommandHandler : ICommandHandler<UpdateUIElementCommand>
    {
        private readonly UIStateService _uiStateService;
        private readonly IUIHubNotifier _hubNotifier;

        public UpdateUIElementCommandHandler(UIStateService uiStateService, IUIHubNotifier hubNotifier)
        {
            _uiStateService = uiStateService;
            _hubNotifier = hubNotifier;
        }

        public async Task Handle(UpdateUIElementCommand command, TraceContext traceContext)
        {
            var fragment = _uiStateService.GetFragment(command.ElementId);
            if (fragment == null)
            {
                // Element not found
                return;
            }

            // Basic ownership check
            if (fragment.OwnerId != traceContext.ModuleId)
            {
                // Or check for shared permissions
                // For now, only the owner can update.
                return; 
            }

            if(command.HtmlContent != null)
            {
                fragment.HtmlContent = command.HtmlContent;
            }
            
            if (command.AttributesToSet != null)
            {
                foreach (var attribute in command.AttributesToSet)
                {
                    fragment.Attributes[attribute.Key] = attribute.Value;
                }
            }
            
            if (command.AttributesToRemove != null)
            {
                foreach (var attributeName in command.AttributesToRemove)
                {
                    fragment.Attributes.Remove(attributeName);
                }
            }
            
            // We only notify if the updated fragment is the currently active one for its slot.
            var activeFragment = _uiStateService.GetActiveFragmentForSlot(fragment.SlotName);
            if (activeFragment?.Id == fragment.Id)
            {
                await _hubNotifier.ElementUpdated(new
                {
                    ElementId = fragment.Id,
                    HtmlContent = fragment.HtmlContent,
                    Attributes = fragment.Attributes
                });
            }        }
    }
}
