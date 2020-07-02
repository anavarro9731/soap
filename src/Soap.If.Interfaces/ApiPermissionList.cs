namespace Soap.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public abstract class ApiPermissionList
    {
        //get the list of permissions in a derived class using this base method
        public List<ApiPermission> ToList
        {
            get
            {
                var list = GetType()
                           .GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                           .Select(field => (ApiPermission)field.GetValue(null))
                           .OrderBy(x => x.DisplayOrder)
                           .ToList();

                return list.ToList();
            }
        }

        public ApiPermission GetById(Guid permissionId)
        {
            return ToList.Single(x => x.Id == permissionId);
        }
    }
}