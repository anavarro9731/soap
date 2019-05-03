namespace Soap.Api.Sso.Domain.Tests.Messages.Ping
{
    using System.Linq;
    using System.Threading;
    using FluentAssertions;
    using Soap.Api.Sso.Domain.Messages.Ping;
    using Soap.Api.Sso.Domain.Tests;
    using Soap.If.MessagePipeline.Models.Aggregates;
    using Xunit;

    public class WhenHttpPinged : Test
    {
        private readonly HttpPingCommand command;

        private readonly HttpPingCommand.PongViewModel pongViewModel;

        public WhenHttpPinged()
        {
            // Arrange
            this.command = new HttpPingCommand(typeof(WhenMsmqPinged).FullName);

            // Act
            Thread.Sleep(1); //in memory it runs so fast, that sometimes this test fails because
            // pong.TimeOfCreationAtOrigin.Should().BeAfter(this.command.PingedAt); is not tr
            this.pongViewModel = this.endPoint.HandleCommand(this.command);
        }

        [Fact]
        public void ItShouldSendAPongCommand()
        {
            var pong = this.endPoint.MessageAggregator.CommandsSent.Single().As<PongCommand>();

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