namespace Soap.Api.Sample.Afs
{
    using System.Collections.Generic;
    using System.Linq;
    using Soap.Api.Sample.Afs.Security;
    using Soap.Interfaces;
    using Soap.Utility.Functions.Extensions;

    public class SecurityInfo : ISecurityInfo
    {
        public List<ApiPermission> ApiPermissions
        {
            get
            {
                return typeof(ApiPermissions).GetFields()
                                             .Where(x => x.FieldType == typeof(ApiPermission))
                                             .Select(x => x.GetValue(null))
                                             .Select(x => x.CastOrError<ApiPermission>())
                                             .ToList();
            }
        }

        public List<Role> BuiltInRoles
        {
            get
            {
                return typeof(Roles).GetFields()
                                    .Where(x => x.FieldType == typeof(Role))
                                    .Select(x => x.GetValue(null))
                                    .Select(x => x.CastOrError<Role>())
                                    .ToList();
            }
        }
    }
}