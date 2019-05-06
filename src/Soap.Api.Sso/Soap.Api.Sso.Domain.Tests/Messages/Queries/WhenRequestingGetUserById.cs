﻿namespace Soap.Api.Sso.Domain.Tests.Messages.Queries
{
    using System.Linq;
    using CircuitBoard.Permissions;
    using Serilog;
    using Soap.Api.Sso.Domain.Messages.Queries;
    using Soap.Api.Sso.Domain.Models.Aggregates;
    using Soap.Api.Sso.Domain.Models.Entities;
    using Soap.Api.Sso.Domain.Models.ValueObjects;
    using Soap.If.Interfaces.Messages;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.If.Utility.PureFunctions.Extensions;
    using Xunit;

    public class WhenRequestingGetUserById : Test
    {
        private readonly ApiQuery<User> getUserById;

        private readonly IUserWithPermissions result;

        public WhenRequestingGetUserById()
        {
            //arrange            
            this.endPoint.AddToDatabase(Aggregates.User1);
            this.getUserById = new GetUserById(Aggregates.User1.id);

            //act
            this.result = this.endPoint.HandleQuery(this.getUserById);
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
            Assert.Equal(this.result?.id, Aggregates.User1.id);
        }
    }
}