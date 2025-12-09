using SoftwareCenter.Core.Data;

namespace SoftwareCenter.Core.Commands.UI
{
    public class ShareUIElementOwnershipCommand : ICommand
    {
        public string ElementId { get; }
        public string TargetModuleId { get; }
        public AccessPermissions Permissions { get; }

        public ShareUIElementOwnershipCommand(string elementId, string targetModuleId, AccessPermissions permissions)
        {
            ElementId = elementId;
            TargetModuleId = targetModuleId;
            Permissions = permissions;
        }
    }
}
