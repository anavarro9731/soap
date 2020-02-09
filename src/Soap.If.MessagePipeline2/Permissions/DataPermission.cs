namespace Soap.MessagePipeline.Permissions
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Permissions;

    public class DataPermission : ApplicationPermission, IPermissionInstance
    {
        public DataPermission(
            List<ScopeReference> permissionScope,
            Guid id,
            string permissionName,
            Guid permissionRequiredToAdministerThisPermission,
            int displayOrder)
            : base(id, permissionName, permissionRequiredToAdministerThisPermission, displayOrder)
        {
            ScopeReferences = permissionScope;
        }

        internal DataPermission()
        {
        }

        public List<ScopeReference> ScopeReferences { get; }
    }
}