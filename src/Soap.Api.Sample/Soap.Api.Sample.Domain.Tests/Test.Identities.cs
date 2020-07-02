namespace Sample.Tests
{
    using System;
    using System.Collections.Generic;
    using Sample.Messages.Commands;
    using Sample.Models.Aggregates;
    using Sample.Models.Constants;
    using Soap.Interfaces;

    public partial class Test
    {
        //all variables in here must remain constant for tests to be correct

        public static class Identities
        {
            public static User UserOne = new User()
            {
                id = Guid.NewGuid(),
                UserName = "User One",
                ApiPermissionGroups = new List<ApiPermissionGroup>()
                {
                    new ApiPermissionGroup()
                    {
                        Name = "Upgrade Database"
                    }
                }
            };
        }
    }
}