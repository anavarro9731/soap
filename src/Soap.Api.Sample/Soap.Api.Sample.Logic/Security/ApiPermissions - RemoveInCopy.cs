//* ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Security
{
    using System.Collections.Generic;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;

    public static partial class ApiPermissions
    {
        /* Beware changing any of these keys will result in removing the permission, if anyone has assigned
        permissions directly in Auth0 which they really shouldn't do, or in the case of custom roles, any replacement
        permission will be missing
        
        The pattern for permission keys is data/operation.
         */

        public static readonly ApiPermission Admin = new ApiPermission("admin-permission", "Admin Permission")
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