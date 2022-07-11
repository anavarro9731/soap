using System;
using System.Collections.Generic;
using Soap.Api.Sample.Messages.Commands;
using Soap.Api.Sample.Messages.Events;
using Soap.Interfaces;

namespace Soap.Api.Sample.Tests
{
    using System.Linq;
    using CircuitBoard;
    using Soap.Utility.Functions.Extensions;

    public static class Roles
    {
        public static readonly Role TestRole = new Role("test-role", "Test Role")
        {
            ApiPermissions = new List<ApiPermission>()
            {
                ApiPermissions.TestApiPermission
            }
        };
    }

    public static class ApiPermissions
    {
        public static readonly ApiPermission TestApiPermission = new ApiPermission("test-api-permission", "Test Api Permission")
        {
            DeveloperPermissions = new List<string>
            {
                nameof(C100v1_Ping),
                nameof(C103v1_StartPingPong),
                nameof(E100v1_Pong),
                nameof(C101v1_UpgradeTheDatabase),
                nameof(C102v1_GetServiceState),
                nameof(C105v1_SendLargeMessage),
                nameof(C106v1_LargeCommand),
                nameof(C104v1_TestUnitOfWork),
                nameof(C107v1_CreateOrUpdateTestDataTypes),
                nameof(C109v1_GetC107FormDataForCreate),
                nameof(C110v1_GetTestDataById),
                nameof(C113v1_GetC107FormDataForEdit),
                nameof(C114v1_DeleteTestDataById)

            },
            Description = "Test Api Permission"
        };
    } 
    
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
