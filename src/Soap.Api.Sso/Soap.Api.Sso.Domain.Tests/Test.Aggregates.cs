namespace Soap.Api.Sso.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Utility;

    public partial class Test
    {
        //all variables in here must remain constant for tests to be correct

        public static class Aggregates
        {
            public static User User1
            {
                get
                {
                    var userId = Guid.Parse("a661872a-3e47-4e6c-a626-952d49b463fc");

                    var passwordHash = SecureHmacHash.CreateFrom(
                        "secret-sauce",
                        12000,
                        "6366163007C4F4A464972A2E7009EB4F4CF518E4BBE3827B208F83993FAE76669FEFE4E922359B156417EC9159C0CB02DA613919B859128373F8F796582F30D0");

                    var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);

                    var email = "lskywalker@rebelalliance.org";

                    var securityToken = SecurityToken.Create(
                        userId,
                        passwordDetails.PasswordHash,
                        new DateTime(DateTime.Today.Year, 1, 1),
                        TimeSpan.FromDays(365),
                        false);

                    var user = new User
                    {
                        Email = email,
                        FullName = "Lucas Skywalker",
                        PasswordDetails = passwordDetails,
                        UserName = email,
                        id = userId,
                        ActiveSecurityTokens = new List<SecurityToken>
                        {
                            securityToken
                        }
                    };

                    return user;
                }
            }
        }
    }
}