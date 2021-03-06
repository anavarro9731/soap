using System;
using System.Collections.Generic;
using Soap.Api.Sample.Messages.Commands;
using Soap.Api.Sample.Messages.Events;
using Soap.Interfaces;

namespace Soap.Api.Sample.Tests
{
    public class SecurityInfo : ISecurityInfo
    {
        public List<ApiPermissionGroup> PermissionGroups { get; } = new List<ApiPermissionGroup>
        {
            new ApiPermissionGroup
            {
                Id = Guid.Parse("8F7DD1BD-F6D7-4379-9089-702EB5DCCA27"),
                Name =   "Ping Pong",
                ApiPermissions = new List<string>
                {
                    nameof(C100v1_Ping),
                    nameof(C103v1_StartPingPong),
                    nameof(E100v1_Pong)
                }
            }
        };
    }
}
