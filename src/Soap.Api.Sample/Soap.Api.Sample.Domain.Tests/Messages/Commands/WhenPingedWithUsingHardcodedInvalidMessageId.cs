﻿namespace Soap.Api.Sample.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using Soap.Api.Sample.Domain.Messages.Commands;
    using Soap.If.Interfaces;
    using Soap.If.MessagePipeline;
    using Soap.If.MessagePipeline.MessagePipeline;
    using Soap.If.MessagePipeline.Models;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenPingedWithUsingHardcodedInvalidMessageId
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly int totalAttempts;

        public WhenPingedWithUsingHardcodedInvalidMessageId()
        {
            // Arrange
            this.command = new PingCommand("someone")
            {
                MessageId = MessagePipeline.Constants.ForceFailBeforeMessageCompletesId
            };

            // Act            
            this.totalAttempts = this.endPoint.AppConfig.NumberOfApiMessageRetries + 1;

            for (var i = 1; i <= this.totalAttempts; i++)
                try
                {
                    this.endPoint.HandleCommand(this.command);
                }
                catch (Exception)
                {
                    // ignored
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
            //have to check NSB directly because this is sent outside of the UnitOfWork
            //and for now there is no message in the aggregator showing this took place
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