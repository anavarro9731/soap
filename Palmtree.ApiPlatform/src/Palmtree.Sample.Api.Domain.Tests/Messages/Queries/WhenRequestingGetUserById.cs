namespace Palmtree.Sample.Api.Domain.Tests.Messages.Queries
{
    using System.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Palmtree.ApiPlatform.DomainTests.Infrastructure;
    using Palmtree.ApiPlatform.Infrastructure.Models;
    using Palmtree.ApiPlatform.Utility.PureFunctions.Extensions;
    using Palmtree.Sample.Api.Domain.Messages.Queries;
    using Palmtree.Sample.Api.Domain.Models.Aggregates;
    using Palmtree.Sample.Api.Domain.Models.Entities;
    using Palmtree.Sample.Api.Domain.Models.ValueObjects;
    using Xunit;

    public class WhenRequestingGetUserById
    {
        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly IApiQuery getUserById;

        private readonly IUserWithPermissions result;

        public WhenRequestingGetUserById()
        {
            //arrange            
            this.endPoint.AddToDatabase(TestData.User1);
            this.getUserById = new GetUserById(TestData.User1.id);

            //act
            this.result = (IUserWithPermissions)this.endPoint.HandleQuery(this.getUserById);
        }

        [Fact]
        public void ItShouldNotCreateAMessageLogEntry()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.getUserById.MessageId))
                                    .Result.SingleOrDefault();

            Assert.Null(logItemResult);
        }

        [Fact]
        public void ItShouldNotLogTheSensitiveUserDetails()
        {
            var logEntries = this.endPoint.MessageAggregator.LogEntries;

            Assert.Equal(1, logEntries.Count());

            var logEntry = logEntries.Single();

            Assert.False(logEntry.Text.Contains($"\"{Objects.GetPropertyName<User>(type => type.PasswordDetails)}\":"));
            Assert.False(logEntry.Text.Contains($"\"{Objects.GetPropertyName<AccountHistory>(type => type.PasswordLastChanged)}\":"));
            Assert.False(logEntry.Text.Contains($"\"{Objects.GetPropertyName<SecurityToken>(type => type.SecureHmacHash)}\":"));
        }

        [Fact]
        public void ItShouldReturnTheUser()
        {
            Assert.Equal(this.result?.id, TestData.User1.id);
        }
    }
}
