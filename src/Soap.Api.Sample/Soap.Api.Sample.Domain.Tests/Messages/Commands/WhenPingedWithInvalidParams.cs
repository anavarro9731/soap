namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenPingedWithInvalidParams
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endpoint = TestEnvironment.CreateEndpoint();

        private readonly int totalAttempts;

        public WhenPingedWithInvalidParams()
        {
            // Arrange
            this.command = new PingCommand(null);

            // Act            
            this.totalAttempts = this.endpoint.AppConfig.NumberOfApiMessageRetries + 1;

            for (var i = 1; i <= this.totalAttempts; i++)
                try
                {
                    this.endpoint.HandleCommand(this.command);
                }
                catch (Exception e)
                {
                    Debug.Write(e.ToString());
                }
        }

        [Fact]
        public void ItShouldNotRecordASuccess()
        {
            var logItemResult = this.endpoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.Null(logItemResult.SuccessfulAttempt);
        }

        [Fact]
        public void ItShouldRecordAllFailedAttempts()
        {
            var logItemResult = this.endpoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.Equal(this.totalAttempts, logItemResult.FailedAttempts.Count);
        }

        [Fact]
        public void ItShouldSendAMaxFailedMessageOnTheLastIteration()
        {
            Assert.Single(this.endpoint.InMemoryMessageBus.Commands.OfType<MessageFailedAllRetries<PingCommand>>());
        }

        [Fact]
        public void ItShouldThrowADifferentErrorOnTheFirstIterationAboveTheMaximumRetries()
        {
            try
            {
                this.endpoint.HandleCommand(this.command);
            }
            catch (PipelineExceptionMessages.PipelineException e)
            {
                Assert.Contains(GlobalErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes.Key, e.Errors);
            }
        }
    }
}