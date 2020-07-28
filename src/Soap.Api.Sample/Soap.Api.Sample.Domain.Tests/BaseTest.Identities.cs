namespace Sample.Tests
{
    using System;
    using System.Collections.Generic;
    using Sample.Models.Aggregates;
    using Soap.Interfaces;

    public partial class BaseTest
    {
        //all variables in here must remain constant for tests to be correct

        protected static class Identities
        {
            public static readonly User UserOne = new User
            {
                id = Guid.NewGuid(),
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
}