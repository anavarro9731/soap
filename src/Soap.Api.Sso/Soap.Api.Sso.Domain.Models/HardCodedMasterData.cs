namespace Soap.Api.Sso.Domain.Models
{
    using System;

    public static class HardCodedMasterData
    {
        public static class RootUser
        {
            public const string EmailAddress = "root@foo.com.uk";

            public const string FullName = "Root User";

            public const string Password = "secret-sauce";

            public const string UserName = "root";

            public static readonly Guid UserId = Guid.Parse("a661872a-3e47-4e6c-a626-952d49b463fc");
        }
    }
}