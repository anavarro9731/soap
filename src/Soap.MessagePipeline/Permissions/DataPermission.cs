namespace Soap.MessagePipeline.Permissions
{
    using System;
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel;
    using Newtonsoft.Json;

    public class DataPermission : ApplicationPermission
    {
        [JsonConstructor]
        public DataPermission(
            IReadOnlyList<IScopeReference> permissionScope,
            Guid id,
            string permissionName,
            Guid permissionRequiredToAdministerThisPermission,
            int displayOrder)
            : base(id, permissionName, permissionRequiredToAdministerThisPermission, displayOrder)
        {
            PermissionScope = permissionScope;
        }

        public IReadOnlyList<IScopeReference> PermissionScope { get; set; }

        public static DataPermission Create(
            IReadOnlyList<IScopeReference> permissionScope,
            Guid id,
            string permissionName,
            Guid permissionRequiredToAdministerThisPermission,
            int displayOrder = 99)
        {
            return new DataPermission(permissionScope, id, permissionName, permissionRequiredToAdministerThisPermission, displayOrder);
        }
    }
}
