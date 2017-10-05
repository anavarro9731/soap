namespace Palmtree.Api.Sso.Domain.Tests.Messages.Queries
{
    using System.Linq;
    using Palmtree.Api.Sso.Domain.Messages.Queries;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.Entities;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Serilog;
    using Serilog.Core;
    using ServiceApi.Interfaces.LowLevel.Messages.InterService;
    using ServiceApi.Interfaces.LowLevel.Permissions;
    using Soap.DomainTests.Infrastructure;
    using Soap.MessagePipeline.Models.Aggregates;
    using Soap.Utility.PureFunctions.Extensions;
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
            Log.CloseAndFlush(); //make sure all entries are accounted for

            var logEntries = this.endPoint.MessageAggregator.LogEntries;
            var logEntry = logEntries.Single(l => l.Text.Contains(this.getUserById.MessageId.ToString()));

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
