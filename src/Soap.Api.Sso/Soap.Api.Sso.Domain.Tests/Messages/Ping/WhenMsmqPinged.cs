namespace Soap.Api.Sso.Domain.Tests.Messages.Ping
{
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Api.Sso.Domain.Tests;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Xunit;

    public class WhenMsmqPinged : Test
    {
        private readonly MsmqPingCommand command;

        public WhenMsmqPinged()
        {
            // Arrange
            this.command = new MsmqPingCommand(typeof(WhenMsmqPinged).FullName);

            // Act
            Thread.Sleep(1); //in memory it runs so fast, that sometimes this test fails because
            // pong.TimeOfCreationAtOrigin.Should().BeAfter(this.command.PingedAt); is not tr
            this.endPoint.HandleCommand(this.command);
        }

        [Fact]
        public void ItShouldReplyWithAPongCommand() 
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

    }
}