namespace Soap.Api.Sample.Domain.Models
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Permissions;
    using Soap.Api.Sample.Domain.Models.ValueObjects;

    public class User : IUserWithPermissions
    {
        public Guid id { get; set; }

        public List<IApplicationPermission> Permissions { get; set; }

        public string UserName { get; set; }

        public bool HasPermission(IApplicationPermission permission)
        {
            return Permissions.Contains(permission);
        }

        public List<SecurityToken> ActiveSecurityTokens { get; set; } = new List<SecurityToken>();

    }
}