//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC105v1CheckAuth : Test
    {
        public TestC105v1CheckAuth(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ItShouldReuseContextHeadersIfThereIsNoServiceLevelAuth()
        {
            Setup();
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be(TestHeaderConstants.IdentityChainHeader);
            c106Headers.GetIdentityToken().Should().Be(TestHeaderConstants.IdentityTokenHeader);
            c106Headers.GetAccessToken().Should().Be(TestHeaderConstants.AccessTokenHeader);
        }
        
        [Fact]
        public void ItShouldUseEnterpriseAdminHeadersIfThereIsServiceLevelAuth()
        {
            Setup(forceServiceLevelAuthOnOutgoingC106:true);
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be($"{TestHeaderConstants.IdentityChainHeader},service://{new TestConfig().AppId}");
            c106Headers.GetIdentityToken().Should().BeNullOrEmpty();
            c106Headers.GetAccessToken().Should().Be(TestHeaderConstants.ServiceLevelAccessTokenHeader);
        }
        
        [Fact]
        public void ItShouldUseNoHeadersIfAuthIsDisabled()
        {
            Setup(disableAuth: true);
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().BeNullOrEmpty();
            c106Headers.GetIdentityToken().Should().BeNullOrEmpty();
            c106Headers.GetAccessToken().Should().BeNullOrEmpty();
        }

        [Fact]
        public void ItShouldSucceedEvenWithNoPermissionsBecauseItHasTheNoAuthAttribute()
        {
            Setup();
            Result.Success.Should().BeTrue();
        }

        private void Setup(bool forceServiceLevelAuthOnOutgoingC106 = false, bool disableAuth = false)
        {
            TestMessage(
                    new C105v1_SendLargeMessage().Op(
                        m =>
                            {
                            if (forceServiceLevelAuthOnOutgoingC106)
                            {
                                m.Headers.SetMessageId(SpecialIds.ForceServiceLevelAuthorityOnOutgoingMessages);
                            }
                            }),
                    Identities.UserOne.Op(x => x.ApiIdentity.ApiPermissions.Clear()),
                    authEnabled: !disableAuth,
                    setupMocks: messageAggregatorForTesting =>
                        {
                        messageAggregatorForTesting.When<BlobStorage.Events.BlobGetSasTokenEvent>().Return("fake-token");
                        messageAggregatorForTesting.When<BlobStorage.Events.BlobUploadEvent>().Return(Task.CompletedTask);
                        })
                .Wait();
        }
    }
}
