namespace Soap.Api.Sample.Logic.Queries
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;

    public class TestDataQueries : Query
    {
        public Func<Guid,Task<TestData>> GetTestData =>
            async (id) => await DataReader.ReadById<TestData>(id);
    }
}
