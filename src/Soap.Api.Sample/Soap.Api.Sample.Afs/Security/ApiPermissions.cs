namespace Soap.Api.Sample.Afs.Security
{
    using System.Collections.Generic;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    
    
    public static class ApiPermissions
    {
        /* Beware changing any of these keys will result in removing the permission, if anyone has assigned
        permissions directly in Auth0 which they really shouldn't do, or in the case of custom roles, any replacement
        permission will be missing */

        public static readonly ApiPermission PingPong__Execute = new ApiPermission("ping-pong/execute", "Ping Pong: Execute")
        {
            Description = "Test Messages",
            DeveloperPermissions = new List<string>
            {
                nameof(C100v1_Ping),
                nameof(C103v1_StartPingPong),
                nameof(E100v1_Pong)
            }
        };

        public static readonly ApiPermission TestData__Read = new ApiPermission("test-data/read", "Test Data: Read")
        {
            DeveloperPermissions = new List<string>
            {
                nameof(C111v1_GetRecentTestData),
                nameof(C110v1_GetTestDataById)
            }
        };

        public static readonly ApiPermission TestData__RestrictedOperations =
            new ApiPermission("test-data/restricted", "Test Data: Restricted Operations")
            {
                DeveloperPermissions = new List<string>
                {
                    nameof(C114v1_DeleteTestDataById),
                    TestDataOperations.CustomDeveloperPermissions.CanHardDelete
                }
            };

        public static readonly ApiPermission TestData__Write = new ApiPermission("test-data/write", "Test Data: Write")
        {
            DeveloperPermissions = new List<string>
            {
                nameof(C109v1_GetC107FormDataForCreate),
                nameof(C107v1_CreateOrUpdateTestDataTypes),
                nameof(C113v1_GetC107FormDataForEdit)
            }
        };
    }
}