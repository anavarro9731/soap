namespace Soap.Api.Sample.Tests
{
    using System.Linq;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;

    //* all variables in here must remain constant for tests to be correct

    public static class Identities
    {
        public static readonly ApiIdentity UserOne = new ApiIdentity
        {
            Id = Ids.ApiIdOne,
            ApiPermissions = typeof(C100v1_Ping).Assembly.GetTypes()
                                                .Where(t => t.InheritsOrImplements(typeof(ApiCommand)))
                                                .Select(t => t.Name)
                                                .ToList()
        };
    }
}
