namespace Soap.Api.Sample.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Api.Sample.Logic.Security;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
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
