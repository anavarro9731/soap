namespace Soap.Api.Sso.Domain.Tests.Messages.Commands
{
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Messages.Commands;
    using Soap.Api.Sso.Domain.Messages.Events;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Soap.Pf.DomainTestsBase;
    using Xunit;

    public class WhenPinged
    {
        private readonly PingCommand command;

        private readonly TestEndpoint endPoint = TestEnvironment.CreateEndpoint();

        private readonly PingCommand.PongViewModel pongViewModel;

        public WhenPinged()
        {
            // Arrange
            this.command = new PingCommand(typeof(WhenPinged).FullName);

            // Act
            Thread.Sleep(1); //in memory it runs so fast, that sometimes this test fails because
            // pong.TimeOfCreationAtOrigin.Should().BeAfter(this.command.PingedAt); is not tr
            this.pongViewModel = this.endPoint.HandleCommand(this.command);
        }

        [Fact]
        public void ItShouldPongAsACommand()
        {
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
}