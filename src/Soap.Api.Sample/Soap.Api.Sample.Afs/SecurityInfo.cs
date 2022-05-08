// ReSharper disable InconsistentNaming

namespace Soap.Api.Sample.Afs
{
    using System.Collections.Generic;
    using CircuitBoard;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;

    

    public class RoleEnum : TypedEnumeration<RoleEnum>
    {
        public static readonly RoleEnum TestDataAnalyst = Create("test-data-analyst", "Test Data Analyst");

        public static readonly RoleEnum TestDataManager = Create("test-data-manager", "Test Data Manager");
    }

    public class ApiPermissionEnum : TypedEnumeration<ApiPermissionEnum>
    {
        /* Beware changing any of these keys will result in removing the permission, if anyone has assigned
        permissions directly in Auth0 which they really shouldn't do, or in the case of custom roles, any replacement
        permission will be missing */ 
        
        public static readonly ApiPermissionEnum PingPong__Execute = Create("ping-pong/execute", "Ping Pong: Execute");

        public static readonly ApiPermissionEnum TestData__Read = Create("test-data/read", "Test Data: Read");

        public static readonly ApiPermissionEnum TestData__Restricted = Create("test-data/restricted", "Test Data: Restricted");

        public static readonly ApiPermissionEnum TestData__Write = Create("test-data/write", "Test Data: Write");
    }

    public class SecurityInfo : ISecurityInfo
    {
        public List<ApiPermission> ApiPermissions { get; } = new List<ApiPermission>
        {
            new ApiPermission
            {
                Id = ApiPermissionEnum.PingPong__Execute,
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
                Id = ApiPermissionEnum.TestData__Read,
                DeveloperPermissions = new List<string>
                {
                    nameof(C111v1_GetRecentTestData),
                    nameof(C110v1_GetTestDataById)
                }
            },
            new ApiPermission
            {
                Id = ApiPermissionEnum.TestData__Write,
                DeveloperPermissions = new List<string>
                {
                    nameof(C109v1_GetC107FormDataForCreate),
                    nameof(C107v1_CreateOrUpdateTestDataTypes),
                    nameof(C113v1_GetC107FormDataForEdit)
                }
            },
            new ApiPermission
            {
                Id = ApiPermissionEnum.TestData__Restricted,
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
                Id = RoleEnum.TestDataAnalyst,
                Description = "Access to View All Test Data",
                ApiPermissions = new List<Enumeration> { ApiPermissionEnum.TestData__Read }
            },
            new Role
            {
                Id = RoleEnum.TestDataManager,
                Description = "Access to Change All Test Data",
                ApiPermissions = new List<Enumeration> { ApiPermissionEnum.TestData__Read, ApiPermissionEnum.TestData__Write }
            }
        };
    }
}