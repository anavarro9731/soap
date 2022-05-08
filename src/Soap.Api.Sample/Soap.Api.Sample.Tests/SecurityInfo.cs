using System;
using System.Collections.Generic;
using Soap.Api.Sample.Messages.Commands;
using Soap.Api.Sample.Messages.Events;
using Soap.Interfaces;

namespace Soap.Api.Sample.Tests
{
    using CircuitBoard;

    public class SecurityInfo : ISecurityInfo
    {
        public List<ApiPermission> ApiPermissions { get; } = new List<ApiPermission>
        {
            new ApiPermission
            {
                Id = new Enumeration("test-api-permission","Test Api Permission"),
                DeveloperPermissions = new List<string>
                {
                    nameof(C100v1_Ping),
                    nameof(C103v1_StartPingPong),
                    nameof(E100v1_Pong)
                }
            }
        };

        public List<Role> BuiltInRoles { get; } = new List<Role>();
    }
}
/* ended thinking about how custom developer permissions would be used in ApiPermissions
but Role => ApiPermission => DeveloperPermission plus scope on Role is great and its basically what you have in Azure portal
it also works when you think about Role Instance having Db Permissions, same principle just sliced by data rather than behaviour
and great for enforcing GDPR multitenancy type situations at the lowest layer */