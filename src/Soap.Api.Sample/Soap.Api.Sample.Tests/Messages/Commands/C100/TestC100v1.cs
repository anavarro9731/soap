namespace Soap.Api.Sample.Tests.Messages.Commands.C100
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Api.Sample.Models.Aggregates;
    using Soap.Idaam;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

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
            (await Result.DataStore.Read<UserProfile>()).Single().IdaamProviderId.Should().Be(Ids.JohnDoeWithAllPermissions.ToIdaam());
        }

    }
}
