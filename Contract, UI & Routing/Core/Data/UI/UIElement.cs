namespace SoftwareCenter.Core.Data.UI
{
    public record UIElement(
        string Id,
        string OwnerId,
        string ElementType,
        string? ParentId = null,
        Dictionary<string, string>? Attributes = null,
        AccessPermissions Permissions = AccessPermissions.None
    );
}
