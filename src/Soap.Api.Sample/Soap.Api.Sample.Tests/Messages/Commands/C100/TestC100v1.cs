namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC100v1 : Test
    {
        public TestC100v1(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.Ping, Identities.UserOne).Wait();
        }

        [Fact]
        public void ItShouldPublishAPongEvent()
        {
            Result.MessageBus.BusEventsPublished.Should().ContainSingle();
            Result.MessageBus.BusEventsPublished.Single().Should().BeOfType<E100v1_Pong>();
            //* events don't have auth headers
            Result.MessageBus.BusEventsPublished.Single()
                  .Headers.Op(
                      h =>
                          {
                          h.GetIdentityChain().Should().BeNullOrEmpty();
                          h.GetAccessToken().Should().BeNullOrEmpty();
                          h.GetIdentityToken().Should().BeNullOrEmpty();
                          });
        }

    }
}
