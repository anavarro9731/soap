namespace Soap.Api.Sso.Endpoint.Tests
{
    using System;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Utility;

    public static class TestData
    {
        public static string GetMessageIdentityToken()
        {
            //all vars in here must be remain the same no matter how many times this is called
            var userId = Guid.Parse("a661872a-3e47-4e6c-a626-952d49b463fc");
            var password = "secret-sauce";
            return GetMessageIdentityToken(userId, password);
        }

        internal static string GetMessageIdentityToken(Guid userId, string password)
        {
            //all vars in here must be remain the same no matter how many times this is called
            var passwordHash = SecureHmacHash.CreateFrom(
                password,
                12000,
                "6366163007C4F4A464972A2E7009EB4F4CF518E4BBE3827B208F83993FAE76669FEFE4E922359B156417EC9159C0CB02DA613919B859128373F8F796582F30D0");
            var passwordDetails = PasswordDetails.Create(passwordHash.IterationsUsed, passwordHash.HexSalt, passwordHash.HexHash);
            var securityToken = SecurityToken.Create(userId, passwordDetails.PasswordHash, new DateTime(DateTime.Today.Year, 1, 1), TimeSpan.FromDays(365), false);
            var identityToken = SecurityToken.EncryptToken(securityToken);
            return identityToken;
        }
    }
}