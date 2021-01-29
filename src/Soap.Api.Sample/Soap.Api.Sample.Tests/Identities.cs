namespace Soap.Api.Sample.Tests
{
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
        public static TestIdentity UserOne => new TestIdentity(
            new ApiIdentity
            {
                Auth0Id = Ids.UserOneAuth0Id,
                ApiPermissions = typeof(C100v1_Ping).Assembly.GetTypes()
                                                    .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                                                    .Select(t => t.Name)
                                                    .ToList()
            },
            new TestProfile(email: "im@mycomputer.com", auth0Id: Ids.UserOneAuth0Id, firstName: "John", lastName: "Doe"));
    }
}
