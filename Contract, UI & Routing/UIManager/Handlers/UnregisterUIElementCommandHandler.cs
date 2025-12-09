using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.UIManager.Services;
using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Handlers
{
    public class UnregisterUIElementCommandHandler : ICommandHandler<UnregisterUIElementCommand>
    {
        private readonly UIStateService _uiStateService;
        private readonly IUIHubNotifier _hubNotifier;

        public UnregisterUIElementCommandHandler(UIStateService uiStateService, IUIHubNotifier hubNotifier)
        {
            _uiStateService = uiStateService;
            _hubNotifier = hubNotifier;
        }

        public async Task Handle(UnregisterUIElementCommand command, TraceContext traceContext)
        {
            var fragmentToRemove = _uiStateService.GetFragment(command.ElementId);
            if (fragmentToRemove == null)
            {
                return;
            }

            // Basic ownership check
            if (fragmentToRemove.OwnerId != traceContext.ModuleId)
            {
                return;
            }

            var slotName = fragmentToRemove.SlotName;
            var wasActive = _uiStateService.GetActiveFragmentForSlot(slotName)?.Id == fragmentToRemove.Id;

            if (_uiStateService.TryRemoveFragment(command.ElementId))
            {
                // If the removed fragment was the active one, we need to notify the client.
                if (wasActive)
                {
                    // First, notify of the removal.
                    await _hubNotifier.ElementRemoved(new { ElementId = command.ElementId });

                    // Then, check for a new fragment to take its place.
                    var newActiveFragment = _uiStateService.GetActiveFragmentForSlot(slotName);
                    if (newActiveFragment != null)
                    {
                        // A new fragment is now the highest priority, tell the client to add it.
                        await _hubNotifier.ElementAdded(new
                        {
                            ElementId = newActiveFragment.Id,
                            ParentId = newActiveFragment.Element.ParentId, // Assuming this is stored
                            HtmlContent = newActiveFragment.HtmlContent,
                        });
                    }
                }
            }
        }
    }
}
