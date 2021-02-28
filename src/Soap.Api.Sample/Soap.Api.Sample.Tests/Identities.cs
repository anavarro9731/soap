namespace Soap.Api.Sample.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using CircuitBoard;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.NotificationServer.Channels;
    using Soap.Utility.Functions.Extensions;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share instances and have concurrency problems if
        one test modifies this. furthermore the identity sent it to the test, is lookedup
        by the auth scheme so any changes you make to it before you send it will be ignored.
        this is by design is makes it more like the production system. I can't protect
        the properties from writing because they need to be serialisable as they are also
        aggregates so just don't change them in tests from how they are defined here. */

    public static class Identities
    {
        public static TestIdentity JaneDoeNoPermissions =>
            new TestIdentity(
                new IdentityPermissions(),
                new TestProfile(
                    Ids.JaneDoeWithNoPermissions,
                    email: "iwas@mycomputer.com",
                    auth0Id: Ids.JaneDoeWithNoPermissionsAuth0Id,
                    firstName: "Jane",
                    lastName: "Doe"));

        public static TestIdentity JohnDoeAllPermissions =>
            new TestIdentity(
                new IdentityPermissions
                {
                    ApiPermissions = typeof(C100v1_Ping).Assembly.GetTypes()
                                                        .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                                                        .Select(t => t.Name)
                                                        .ToList()
                },
                new TestProfile(
                    Ids.JohnDoeWithAllPermissions,
                    email: "im@mycomputer.com",
                    auth0Id: Ids.JohnDoeWithAllPermissionsAuth0Id,
                    firstName: "John",
                    lastName: "Doe"));

        public static List<TestIdentity> TestIdentities
        {
            get
            {
                var identities = typeof(Identities).GetProperties()
                                                   .Where(x => x.PropertyType == typeof(TestIdentity))
                                                   .Select(x => x.GetValue(null, null))
                                                   .Select(y => y.DirectCast<TestIdentity>())
                                                   .ToList();

                return identities;
            }
        }
    }
}
