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
        public void ItShouldReuseContextHeadersOnC106ByDefault()
        {
            Setup();
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be(Identities.UserOne.IdentityChainSegment);
            c106Headers.GetIdentityToken().Should().Be(TestHeaderConstants.IdentityTokenHeader);
            c106Headers.GetAccessToken().Should().Be(TestHeaderConstants.AccessTokenHeader);
        }
        
        [Fact]
        public void ItShouldSetEnterpriseAdminHeadersOnC106IfServiceLevelAuthIsRequested()
        {
            Setup(forceServiceLevelAuthOnOutgoingC106:true);
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be($"{Identities.UserOne.IdentityChainSegment},service://{new TestConfig().AppId}");
            c106Headers.GetIdentityToken().Should().BeNullOrEmpty();
            c106Headers.GetAccessToken().Should().Be(TestHeaderConstants.ServiceLevelAccessTokenHeader);
        }
        
        [Fact]
        public void ItShouldSetEnterpriseAdminHeadersOnC106IfUseServiceLevelAuthWhenThereIsNoSecurityContextHasBeenEnabled()
        {
            Setup(enableSlaWhenSecurityContextIsAbsent:true);
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be($"service://{new TestConfig().AppId}");
            c106Headers.GetIdentityToken().Should().BeNullOrEmpty();
            c106Headers.GetAccessToken().Should().Be(TestHeaderConstants.ServiceLevelAccessTokenHeader);
        }
        
        [Fact]
        public void ItShouldSetNoHeadersOnC106IfAuthIsDisabled()
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

        private void Setup(bool forceServiceLevelAuthOnOutgoingC106 = false, bool disableAuth = false, bool enableSlaWhenSecurityContextIsAbsent = false)
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
                    identity: enableSlaWhenSecurityContextIsAbsent ? null : Identities.UserOne.Op(x => x.ApiIdentity.ApiPermissions.Clear()),
                    authEnabled: !disableAuth,
                    setupMocks: messageAggregatorForTesting =>
                        {
                        messageAggregatorForTesting.When<BlobStorage.Events.BlobGetSasTokenEvent>().Return("fake-token");
                        messageAggregatorForTesting.When<BlobStorage.Events.BlobUploadEvent>().Return(Task.CompletedTask);
                        }, enableSlaWhenSecurityContextIsMissing:enableSlaWhenSecurityContextIsAbsent)
                .Wait();
        }
    }
}
