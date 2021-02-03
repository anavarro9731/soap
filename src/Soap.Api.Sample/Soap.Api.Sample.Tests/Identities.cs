namespace Soap.Api.Sample.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    /* all variables in here must remain constant for tests to be correct,
        HOWEVER they must ALWAYS use arrows and not equal on the property assignment because
        they are static and you will share  instances and have concurrency problems if
        one test modifies this */

    public static class Identities
    {
        public static List<TestIdentity> TestIdentities =>
            typeof(Identities).GetType()
                              .GetProperties()
                              .Where(x => x.Name != nameof(TestIdentities))
                              .Select(x => x.GetValue(null, null).As<TestIdentity>())
                              .ToList();

        public static TestIdentity UserOne =>
            new TestIdentity(
                new IdentityPermissions
                {
                    ApiPermissions = typeof(C100v1_Ping).Assembly.GetTypes()
                                                        .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                                                        .Select(t => t.Name)
                                                        .ToList()
                },
                new TestProfile(
                    Ids.UserOneId,
                    email: "im@mycomputer.com",
                    auth0Id: Ids.UserOneAuth0Id,
                    firstName: "John",
                    lastName: "Doe"));
    }
}
