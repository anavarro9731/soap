using System.Collections.Generic;
using Soap.Api.Sample.Messages.Commands;
using Soap.Api.Sample.Messages.Events;
using Soap.Interfaces;

public class SecurityInfo : ISecurityInfo
{
    public List<ApiPermissionGroup> PermissionGroups { get; } = new List<ApiPermissionGroup>
    {
        new ApiPermissionGroup
        {
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
