//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C100
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;
    using Commands = Soap.Api.Sample.Tests.Commands;

    public class TestC100v1WithAuthLevelNone : Test
    {
        public TestC100v1WithAuthLevelNone(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            TestMessage(Commands.Ping, Identities.JaneDoeNoPermissions, authLevel: AuthLevel.None).Wait();
        }

        [Fact]
        public void ItShouldNotCareAboutTheLackOfPermissions()
        {
            Result.Success.Should().BeTrue();

        }
        
        [Fact]
        public void ThePongEventShouldNotHaveAnyAuthHeaders()
        {
            //* because events don't have auth headers
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
