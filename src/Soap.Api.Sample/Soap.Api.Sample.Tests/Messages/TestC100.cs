namespace Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC100 : Test
    {
        public TestC100(ITestOutputHelper outputHelper)
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