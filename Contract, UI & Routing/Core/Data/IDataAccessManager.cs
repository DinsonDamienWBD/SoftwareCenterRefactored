using System.Collections.Generic;
using SoftwareCenter.Core.Data; // For AccessPermissions

namespace SoftwareCenter.Core.Data
{
    /// <summary>
    /// Represents the metadata of a stored item, used by the data access manager for permission checks.
    /// </summary>
    public class StoreItemMetadata
    {
        public string OwnerModuleId { get; set; }
        public Dictionary<string, AccessPermissions> SharedPermissions { get; set; }
    }

    /// <summary>
    /// Defines a contract for managing access permissions to data items.
    /// </summary>
    public interface IDataAccessManager
    {
        /// <summary>
        /// Checks if a requesting module has the required permission to access a data item.
        /// </summary>
        /// <param name="itemMetadata">The metadata of the stored item, including owner and shared permissions.</param>
        /// <param name="requestingModuleId">The ID of the module attempting to access the data.</param>
        /// <param name="requiredPermission">The specific permission required for the operation.</param>
        /// <returns>True if the module has the required permission, false otherwise.</returns>
        bool CheckPermission(StoreItemMetadata itemMetadata, string requestingModuleId, AccessPermissions requiredPermission);

        /// <summary>
        /// Determines if a requesting module is the owner of the data item.
        /// </summary>
        /// <param name="itemMetadata">The metadata of the stored item.</param>
        /// <param name="requestingModuleId">The ID of the module attempting to access the data.</param>
        /// <returns>True if the module is the owner, false otherwise.</returns>
        bool IsOwner(StoreItemMetadata itemMetadata, string requestingModuleId);

        /// <summary>
        /// Checks if a requesting module has administrative privileges.
        /// </summary>
        /// <param name="requestingModuleId">The ID of the module to check.</param>
        /// <returns>True if the module is an administrator, false otherwise.</returns>
        bool IsAdmin(string requestingModuleId);
    }
}
