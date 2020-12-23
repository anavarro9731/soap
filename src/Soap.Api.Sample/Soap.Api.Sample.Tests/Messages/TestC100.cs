namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC100 : Test
    {
        public TestC100(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByProcessingAMessage(Commands.Ping, Identities.UserOne);
        }

        [Fact]
        public void ItShouldPublishAPongEvent()
        {
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.BusEventsPublished.Single().Should().BeOfType<E100v1_Pong>();
        }
    }
}