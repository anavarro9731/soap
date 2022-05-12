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
                Id = new ApiPermissionId("test-api-permission","Test Api Permission"),
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
