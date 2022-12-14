//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.Queries.Predicates
{
    using System;
    using System.Linq.Expressions;
    using Soap.Utility.Functions.Extensions;

    internal static class TestData
    {
        public static Expression<Func<Models.Aggregates.TestData, bool>> RecentTestData(long daysBack) =>
            data => data.CreatedAsMillisecondsEpochTime > DateTime.UtcNow.ConvertToMillisecondsEpochTime() - checked(86400000 * daysBack);
    }
}
