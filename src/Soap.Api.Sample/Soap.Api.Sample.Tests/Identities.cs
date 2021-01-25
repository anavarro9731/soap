namespace Soap.Api.Sample.Tests
{
    using System.Collections.Generic;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces;

    //* all variables in here must remain constant for tests to be correct

    public static class Identities
    {
        public static readonly ApiIdentity UserOne = new ApiIdentity()
        {
            Id = Ids.ApiIdOne,
        };
    }
}
