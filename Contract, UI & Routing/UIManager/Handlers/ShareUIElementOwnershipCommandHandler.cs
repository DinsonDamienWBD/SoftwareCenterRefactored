using SoftwareCenter.Core.Commands;
using SoftwareCenter.Core.Commands.UI;
using SoftwareCenter.Core.Diagnostics;
using SoftwareCenter.UIManager.Services;
using System.Threading.Tasks;

namespace SoftwareCenter.UIManager.Handlers
{
    public class ShareUIElementOwnershipCommandHandler : ICommandHandler<ShareUIElementOwnershipCommand>
    {
        private readonly UIStateService _uiStateService;

        public ShareUIElementOwnershipCommandHandler(UIStateService uiStateService)
        {
            _uiStateService = uiStateService;
        }

        public Task Handle(ShareUIElementOwnershipCommand command, TraceContext traceContext)
        {
            var fragment = _uiStateService.GetFragment(command.ElementId);
            if (fragment == null)
            {
                return Task.CompletedTask;
            }

            // Only the owner can share.
            if (fragment.OwnerId != traceContext.ModuleId)
            {
                return Task.CompletedTask;
            }

            // This is not ideal as it modifies a 'supposedly' immutable object.
            // A better implementation would involve a more structured permissions model.
            // For now, we'll just update the property on the existing element.
            // To do this properly, UIElement should probably be a class or we need a different state update mechanism.

            var currentElement = fragment.Element;
            var newPermissions = currentElement.Permissions | command.Permissions; // Add new permissions

            // Since UIElement is a record, we create a new one.
            var updatedElement = currentElement with { Permissions = newPermissions };

            // And we update the fragment.
            fragment.Element = updatedElement;

            // Note: This change is only in memory. We are not notifying the client as ownership is a backend concept.

            return Task.CompletedTask;
        }
    }
}
