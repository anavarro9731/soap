namespace Palmtree.Api.Sso.Domain.Tests
{
    using System;
    using System.Linq;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Soap.Utility;

    public static class TestData
    {
        public static User User1
        {
            get
            {
                //all vars in here must be remain the same no matter how many times this is called
                var userId = Guid.Parse("a661872a-3e47-4e6c-a626-952d49b463fc");
                var passwordHash = SecureHmacHash.CreateFrom(
                    "secret-sauce",
                    12000,
                    "6366163007C4F4A464972A2E7009EB4F4CF518E4BBE3827B208F83993FAE76669FEFE4E922359B156417EC9159C0CB02DA613919B859128373F8F796582F30D0");
                var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);
                var user = User.Create(
                    "monmotha@rebelalliance.org",
                    "Mon Monthma",
                    passwordDetails,
                    new IApplicationPermission[]
                    {
                    }.ToList(),
                    "monmothma",
                    userId);
                var securityToken = SecurityToken.Create(
                    user.id,
                    user.PasswordDetails.PasswordHash,
                    new DateTime(DateTime.Today.Year, 1, 1),
                    TimeSpan.FromDays(365),
                    false);
                user.ActiveSecurityTokens.Add(securityToken);
                return user;
            }
        }

        public static string GetIdentityToken(this User user)
        {
            return ClientSecurityContext.Create(user.ActiveSecurityTokens.First(), user).AuthToken;
        }
    }
}
