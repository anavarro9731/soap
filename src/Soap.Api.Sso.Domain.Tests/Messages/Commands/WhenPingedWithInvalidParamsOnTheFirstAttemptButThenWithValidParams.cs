namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenPingedWithInvalidParamsOnTheFirstAttemptButThenWithValidParams
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenPingedWithInvalidParamsOnTheFirstAttemptButThenWithValidParams()
        {
            // Arrange
            this.command = new PingCommand("SpecialInvalidParamSeeCodeInHandler");

            // Act            

            try
            {
                this.endPoint.HandleCommand(this.command);
            }
            catch (Exception)
            {
                this.endPoint.HandleCommand(this.command);
            }
        }

        [Fact]
        public void ItShouldRecordAllFailedAttempts()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.Single(logItemResult.FailedAttempts);
        }

        [Fact]
        public void ItShouldRecordASuccess()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.NotNull(logItemResult.SuccessfulAttempt);
        }
    }
}