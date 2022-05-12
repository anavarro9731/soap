// ReSharper disable InconsistentNaming

namespace Soap.Api.Sample.Afs
{
    using System.Collections.Generic;
    using CircuitBoard;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;

    public static class RolesIds
    {
        public static readonly RoleId TestDataAnalyst = new RoleId("test-data-analyst", "Test Data Analyst");

        public static readonly RoleId TestDataManager = new RoleId("test-data-manager", "Test Data Manager");
    }

    public static class ApiPermissionIds
    {
        /* Beware changing any of these keys will result in removing the permission, if anyone has assigned
        permissions directly in Auth0 which they really shouldn't do, or in the case of custom roles, any replacement
        permission will be missing */

        public static readonly ApiPermissionId PingPong__Execute = new ApiPermissionId("ping-pong/execute", "Ping Pong: Execute");

        public static readonly ApiPermissionId TestData__Read = new ApiPermissionId("test-data/read", "Test Data: Read");

        public static readonly ApiPermissionId TestData__Restricted = new ApiPermissionId("test-data/restricted", "Test Data: Restricted");

        public static readonly ApiPermissionId TestData__Write = new ApiPermissionId("test-data/write", "Test Data: Write");
    }

    public class SecurityInfo : ISecurityInfo
    {
        public List<ApiPermission> ApiPermissions { get; } = new List<ApiPermission>
        {
            new ApiPermission
            {
                Id = ApiPermissionIds.PingPong__Execute,
                Description = "Test Messages",
                DeveloperPermissions = new List<string>
                {
                    nameof(C100v1_Ping),
                    nameof(C103v1_StartPingPong),
                    nameof(E100v1_Pong)
                }
            },
            new ApiPermission
            {
                Id = ApiPermissionIds.TestData__Read,
                DeveloperPermissions = new List<string>
                {
                    nameof(C111v1_GetRecentTestData),
                    nameof(C110v1_GetTestDataById)
                }
            },
            new ApiPermission
            {
                Id = ApiPermissionIds.TestData__Write,
                DeveloperPermissions = new List<string>
                {
                    nameof(C109v1_GetC107FormDataForCreate),
                    nameof(C107v1_CreateOrUpdateTestDataTypes),
                    nameof(C113v1_GetC107FormDataForEdit)
                }
            },
            new ApiPermission
            {
                Id = ApiPermissionIds.TestData__Restricted,
                DeveloperPermissions = new List<string>
                {
                    nameof(C114v1_DeleteTestDataById),
                    TestDataOperations.CustomDeveloperPermissions.CanHardDelete
                }
            }
        };

        public List<Role> BuiltInRoles { get; } = new List<Role>
        {
            new Role
            {
                Id = RolesIds.TestDataAnalyst,
                Description = "Access to View All Test Data",
                ApiPermissions = new List<ApiPermissionId> { ApiPermissionIds.TestData__Read }
            },
            new Role
            {
                Id = RolesIds.TestDataManager,
                Description = "Access to Change All Test Data",
                ApiPermissions = new List<ApiPermissionId> { ApiPermissionIds.TestData__Read, ApiPermissionIds.TestData__Write }
            }
        };
    }
}