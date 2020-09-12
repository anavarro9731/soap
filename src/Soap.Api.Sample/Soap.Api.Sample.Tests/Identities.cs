namespace Soap.Api.Sample.Tests
{
    using System.Collections.Generic;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces;

    //* all variables in here must remain constant for tests to be correct

    public static class Identities
    {
        public static readonly User UserOne = new User
        {
            id = Ids.UserOne,
            UserName = "User One",
            ApiPermissionGroups = new List<ApiPermissionGroup>
            {
                new ApiPermissionGroup
                {
                    Name = "Upgrade Database"
                }
            }
        };
    }
}