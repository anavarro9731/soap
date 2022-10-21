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
        
        public static readonly ApiPermission BuiltInMessages = new ApiPermission("builtin-messages", "BuiltIn Messages")
        {
            DeveloperPermissions = new List<string>
            {
                nameof(C100v1_Ping),
                nameof(C101v1_UpgradeTheDatabase),
                nameof(C102v1_GetServiceState),
                nameof(C103v1_StartPingPong),
                nameof(E100v1_Pong),
                nameof(C115v1_OnStartup),
                nameof(C116v1_TruncateMessageLog)
            },
            Description = "BuiltIn Messages That Can Be Executed By A User"
        };


        
    }
}