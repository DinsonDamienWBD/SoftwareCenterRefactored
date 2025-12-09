using SoftwareCenter.Core.Data;
using System.Collections.Generic;

namespace SoftwareCenter.Kernel.Services
{
    /// <summary>
    /// Default implementation of <see cref="IDataAccessManager"/> that handles permission checks
    /// based on ownership and shared permissions stored in <see cref="StoreItemMetadata"/>.
    /// </summary>
    public class DefaultDataAccessManager : IDataAccessManager
    {
        private const string AdminModuleId = "Admin"; // A hardcoded admin module ID

        public bool CheckPermission(StoreItemMetadata itemMetadata, string requestingModuleId, AccessPermissions requiredPermission)
        {
            if (itemMetadata == null) return false;
            if (requestingModuleId == null) return false;

            // Admin has all permissions
            if (IsAdmin(requestingModuleId))
            {
                return true;
            }

            // Owner always has all permissions
            if (IsOwner(itemMetadata, requestingModuleId))
            {
                return true;
            }

            // Check shared permissions
            if (itemMetadata.SharedPermissions != null && itemMetadata.SharedPermissions.TryGetValue(requestingModuleId, out var grantedPermissions))
            {
                return grantedPermissions.HasFlag(requiredPermission);
            }

            return false;
        }

        public bool IsOwner(StoreItemMetadata itemMetadata, string requestingModuleId)
        {
            if (itemMetadata == null || requestingModuleId == null) return false;
            return itemMetadata.OwnerModuleId == requestingModuleId;
        }

        public bool IsAdmin(string requestingModuleId)
        {
            return requestingModuleId == AdminModuleId;
        }
    }
}
