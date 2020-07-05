namespace Sample.Tests.Messages.Queries
{
    using System.Linq;
    using FluentAssertions;
    using Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class C102GetServiceState : Test
    {
        public C102GetServiceState(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            Execute(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
            Execute(Queries.GetServiceState, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSendAGotMessageEvent()
        {
            Result.MessageBus.Events.Should().ContainSingle();
            Result.MessageBus.Events.Single().Should().BeOfType<E151GotServiceState>();
        }
    }
}