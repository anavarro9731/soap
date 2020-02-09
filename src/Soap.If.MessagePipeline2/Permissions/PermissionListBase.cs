namespace Soap.MessagePipeline.Permissions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using CircuitBoard.Permissions;

    public abstract class PermissionListBase : IPermissionList
    {
        //get the list of permissions in a derived class using this base method
        public List<IPermission> ToList
        {
            get
            {
                var list = GetType()
                    .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                    .Select(field => (IPermission)field.GetValue(null))
                    .OrderBy(x => x.DisplayOrder)
                    .ToList();

                return list.ToList();
            }
        }

        public IPermission GetById(Guid permissionId)
        {
            return ToList.Single(x => x.Id == permissionId);
        }
    }
}