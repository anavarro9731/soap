namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC100v1 : Test
    {
        public TestC100v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.Ping, Identities.JohnDoeAllPermissions).Wait();
        }

        [Fact]
        public void ItShouldPublishAPongEvent()
        {
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.BusEventsPublished.Single().Should().BeOfType<E100v1_Pong>();

        }
        
        [Fact]
        public async void ItShouldCreateTheUserProfile()
        {
            (await Result.DataStore.Read<TestProfile>()).Single().Auth0Id.Should().Be(Ids.JohnDoeWithAllPermissionsAuth0Id);
        }

    }
}
