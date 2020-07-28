namespace Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC102 : BaseTest
    {
        public TestC102(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
            Execute(Queries.GetServiceState, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSendAGotMessageEvent()
        {
            Result.MessageBus.EventsPublished.Should().ContainSingle();
            Result.MessageBus.EventsPublished.Single().Should().BeOfType<E151GotServiceState>();
        }
    }
}