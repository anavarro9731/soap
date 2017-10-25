namespace Palmtree.Api.Sso.Domain.Tests.Messages.Commands
{
    using System;
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Palmtree.Api.Sso.Domain.Messages.Commands;
    using Palmtree.Api.Sso.Domain.Messages.Events;
    using Palmtree.Api.Sso.Domain.Models.ViewModels;
    using Soap.DomainTests.Infrastructure;
    using Soap.Interfaces;
    using Soap.MessagePipeline.MessagePipeline;
    using Soap.MessagePipeline.Models;
    using Soap.MessagePipeline.Models.Aggregates;
    using Xunit;

    public class WhenPinged
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly PongViewModel pongViewModel;

        public WhenPinged()
        {
            // Arrange
            this.command = new PingCommand(typeof(WhenPinged).FullName);

            // Act
            Thread.Sleep(1); //in memory it runs so fast, that sometimes this test fails because
            // pong.TimeOfCreationAtOrigin.Should().BeAfter(this.command.PingedAt); is not true
            this.pongViewModel = this.endPoint.HandleCommand(this.command) as PongViewModel;
        }

        [Fact]
        public void ItShouldPongAsACommand()
        {
            this.endPoint.MessageAggregator.StateOperations.OfType<ISendCommandOperation>().Count().Should().Be(1);

            var pong = this.endPoint.MessageAggregator.CommandsSent.Single().As<PongCommand>();

            pong.Should().NotBeNull();
            pong.TimeOfCreationAtOrigin.Should().BeAfter(this.command.PingedAt);
            pong.PingedAt.Should().Be(this.command.PingedAt);
            pong.PingedBy.Should().Be(this.command.PingedBy);
        }

        [Fact]
        public void ItShouldPongAsAnEvent()
        {
            this.endPoint.MessageAggregator.EventsPublished.Count().Should().Be(1);

            var @event = this.endPoint.MessageAggregator.EventsPublished.Single();

            var pong = @event.As<PongEvent>();
            pong.Should().NotBeNull();
            pong.TimeOfCreationAtOrigin.Should().BeAfter(this.command.PingedAt);
            pong.PingedAt.Should().Be(this.command.PingedAt);
            pong.PingedBy.Should().Be(this.command.PingedBy);
        }

        [Fact]
        public void ItShouldRecordASuccessfulResult()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();
            Assert.NotNull(logItemResult.SuccessfulAttempt);
        }

        [Fact]
        public void ItShouldReturnAPongViewModel()
        {
            this.pongViewModel.Should().NotBeNull();
            this.pongViewModel.PingedAt.Should().Be(this.command.PingedAt);
            this.pongViewModel.PingedBy.Should().Be(this.command.PingedBy);
        }
    }

    public class WhenPingedMultipeTimesWithADifferentMessageButSameMessageId
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenPingedMultipeTimesWithADifferentMessageButSameMessageId()
        {
            // Arrange
            this.command = new PingCommand("ABC");

            var result = this.endPoint.HandleCommand(this.command);
        }

        [Fact]
        public void ItShouldRecordASuccessForTheFirstCommand()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.NotNull(logItemResult.SuccessfulAttempt);
        }

        [Fact]
        public void ItShouldThrowAnErrorForTheSecondCommand()
        {
            try
            {
                this.endPoint.HandleCommand(
                    new PingCommand("DEF")
                    {
                        MessageId = this.command.MessageId
                    });
            }
            catch (Exception e)
            {
                Assert.StartsWith(PipelineExceptionMessages.CodePrefixes.INVALID, e.Message);
            }
        }
    }

    public class WhenPingedMultipeTimesWithTheSameMessage
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        public WhenPingedMultipeTimesWithTheSameMessage()
        {
            // Arrange
            this.command = new PingCommand("ABC");

            this.endPoint.HandleCommand(this.command);
        }

        [Fact]
        public void ItShouldRecordASuccessForTheFirstCommand()
        {
            var logItemResult = this.endPoint.QueryDatabase<MessageLogItem>(query => query.Where(logItem => logItem.id == this.command.MessageId)).Result.Single();

            Assert.NotNull(logItemResult.SuccessfulAttempt);
        }

        [Fact]
        public void ItShouldThrowAnErrorForTheSecondCommand()
        {
            try
            {
                this.endPoint.HandleCommand(this.command);
            }
            catch (Exception e)
            {
                Assert.StartsWith(PipelineExceptionMessages.CodePrefixes.INVALID, e.Message);
            }
        }
    }

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
                catch (Exception)
                {
                    // ignored
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
            catch (Exception e)
            {
                Assert.StartsWith(PipelineExceptionMessages.CodePrefixes.INVALID, e.Message);
            }
        }
    }

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
            catch (Exception e)
            {
                Assert.StartsWith(PipelineExceptionMessages.CodePrefixes.INVALID, e.Message);
            }
        }
    }

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