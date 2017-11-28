﻿namespace Palmtree.Api.Sso.Domain.Tests.Messages.Queries
{
    using System.Linq;
    using CircuitBoard.Permissions;
    using Palmtree.Api.Sso.Domain.Messages.Queries;
    using Palmtree.Api.Sso.Domain.Models.Aggregates;
    using Palmtree.Api.Sso.Domain.Models.Entities;
    using Palmtree.Api.Sso.Domain.Models.ValueObjects;
    using Serilog;
    using Soap.DomainTests.Infrastructure;
    using Soap.Interfaces.Messages;
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

            Assert.DoesNotContain($"\"{Objects.GetPropertyName<User>(type => type.PasswordDetails)}\":", logEntry.Text);
            Assert.DoesNotContain($"\"{Objects.GetPropertyName<AccountHistory>(type => type.PasswordLastChanged)}\":", logEntry.Text);
            Assert.DoesNotContain($"\"{Objects.GetPropertyName<SecurityToken>(type => type.SecureHmacHash)}\":", logEntry.Text);
        }

        [Fact]
        public void ItShouldReturnTheUser()
        {
            Assert.Equal(this.result?.id, TestData.User1.id);
        }
    }
}