namespace Soap.Api.Sso.Domain.Tests.Messages.Ping
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Xunit;

    public class WhenPingedWithInvalidParams : Test
    {
        private readonly PingCommand command;

        private readonly int totalAttempts;

        public WhenPingedWithInvalidParams()
        {
            // Arrange
            this.command = new PingCommand(null);

            // Act            
            this.totalAttempts = this.endPoint.AppConfig.NumberOfApiMessageRetries + 1;

            for (var i = 1; i <= this.totalAttempts; i++)
                try
                {
                    this.endPoint.HandleCommand(this.command);
                }
                catch (Exception e)
                {
                    Debug.Write(e.ToString());
                }
        }

        [Fact]
        public void ItShouldNotRecordASuccess()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.Null(logItemResult.SuccessfulAttempt);
        }

        [Fact]
        public void ItShouldRecordAllFailedAttempts()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.Equal(this.totalAttempts, logItemResult.FailedAttempts.Count);
        }

        [Fact]
        public void ItShouldSendAMaxFailedMessageOnTheLastIteration()
        {
            Assert.Single(this.endPoint.InMemoryMessageBus.Commands.OfType<MessageFailedAllRetries<PingCommand>>());
        }

        [Fact]
        public void ItShouldThrowADifferentErrorOnTheFirstIterationAboveTheMaximumRetries()
        {
            try
            {
                this.endPoint.HandleCommand(this.command);
            }
            catch (PipelineExceptionMessages.PipelineException e)
            {
                Assert.Contains(GlobalErrorCodes.MessageAlreadyFailedMaximumNumberOfTimes.Key, e.Errors);
            }
        }
    }
}