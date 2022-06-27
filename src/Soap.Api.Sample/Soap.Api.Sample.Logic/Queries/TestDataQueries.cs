namespace Soap.Api.Sample.Logic.Queries
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Context;

    public class TestDataQueries : Query
    {
        public Func<Guid, Task<TestData>> GetTestDataById => async id => await DataReader.ReadById<TestData>(id);

        public Func<Task<List<TestData>>> GetRecentTestData(long maxAgeInDays, long maxRecords) =>
            async () =>
                {
                var continueAt = new ContinuationToken();
                return (await DataReader.WithoutEventReplay.ReadActive(
                            Predicates.TestData.RecentTestData(maxAgeInDays),
                            o => o.Take((int)maxRecords, ref continueAt))).OrderByDescending(x => x.CreatedAsMillisecondsEpochTime)
                                                                      .ToList();
                };
    }
}
