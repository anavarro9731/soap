namespace Sample.Tests.Messages.Commands.Ping
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class C100PingPong : Test
    {
        public C100PingPong(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.Ping, Identities.UserOne);
        }

        [Fact]
        public void ItShouldPublishAPongEvent()
        {
            Result.MessageBus.EventsPublished.Should().ContainSingle();
            Result.MessageBus.EventsPublished.Single().Should().BeOfType<E150Pong>();
        }
    }
}