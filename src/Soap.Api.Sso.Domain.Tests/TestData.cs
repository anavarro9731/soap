namespace Soap.Api.Sso.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Utility;

    public static class TestData
    {
        //variables in here must remain constant for tests to be correct
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

        public static string GetIdentityToken(this User user)
        {
            return SecurityToken.EncryptToken(user.ActiveSecurityTokens.First());
        }

        public static class Commands
        {
            public static AddFullyRegisteredUser CreateRegisteredUser1(
                string email = "admiralackbar@rebelalliance.org",
                string name = "Adm. Ackbar",
                string password = "itsatrap")
            {
                return new AddFullyRegisteredUser(Guid.Parse("33de11ce-2058-49bb-a4e9-e1b23fb0b9c4"), email, name, password);
            }

            public static AddFullyRegisteredUser CreateRegisteredUser2(
                string email = "monmotha @rebelalliance.org",
                string name = "Mon Monthma",
                string password = "rebels4ever")
            {
                return new AddFullyRegisteredUser(Guid.Parse("e22449a0-4ecc-4aec-b3de-dc5f8e9acb9e"), email, name, password);
            }
        }
    }
}