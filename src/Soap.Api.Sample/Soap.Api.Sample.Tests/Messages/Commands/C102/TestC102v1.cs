
namespace Soap.Api.Sample.Tests.Messages.Commands.C102
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC102v1 : Test
    {
        public TestC102v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByProcessingAMessage(Commands.UpgradeTheDatabaseToV1, Identities.JohnDoeAllPermissions);
            
            TestMessage(Commands.GetServiceState, Identities.JohnDoeAllPermissions).Wait();
        }

        [Fact]
        public void ItShouldSendAGotMessageEvent()
        {
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.BusEventsPublished.Single().Should().BeOfType<E101v1_GotServiceState>();
        }
    }
}
