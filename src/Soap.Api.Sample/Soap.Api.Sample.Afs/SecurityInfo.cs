using System;
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
            Id = Guid.Parse("8F7DD1BD-F6D7-4379-9089-702EB5DCCA27"),
            Name =   "Ping Pong",
            ApiPermissions = new List<string>
            {
                nameof(C100v1_Ping),
                nameof(C103v1_StartPingPong),
                nameof(E100v1_Pong)
            }
        },
        new ApiPermissionGroup()
        {
            Id = Guid.Parse("C9BF5807-56F1-4FC3-A49E-CCB93218F82C"),
            Name = "Test Data",
            ApiPermissions = new List<string>
            {
                nameof(C111v1_GetRecentTestData),
                nameof(C110v1_GetTestDataById),
                nameof(C109v1_GetC107DefaultFormData),
                nameof(C114v1_DeleteTestDataById),
                nameof(C113v1_GetC107FormDataForEdit)
            }
        }
    };
}
