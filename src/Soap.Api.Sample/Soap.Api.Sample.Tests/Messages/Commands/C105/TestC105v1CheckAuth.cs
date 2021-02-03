//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Config;
    using Soap.Context;
    using Soap.Context.BlobStorage;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Soap.Utility.Functions.Extensions;
    using Soap.Utility.Functions.Operations;
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
            c106Headers.GetIdentityChain().Should().Be(Identities.UserOne.IdChainSegment);
            c106Headers.GetIdentityToken().Should().Be(AesOps.Encrypt(Identities.UserOne.UserProfile.id.ToString(), new TestConfig().EncryptionKey));
            c106Headers.GetAccessToken().Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void ItShouldSlaHeadersOnC106IfServiceLevelAuthIsRequested()
        {
            Setup(forceServiceLevelAuthOnOutgoingC106:true);
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be($"{Identities.UserOne.IdChainSegment},service://{new TestConfig().AppId}");
            c106Headers.GetIdentityToken().Should().BeNullOrEmpty();
            c106Headers.GetAccessToken().Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void ItShouldSetSlaHeadersOnC106IfUseServiceLevelAuthWhenThereIsNoSecurityContextHasBeenEnabled()
        {
            Setup(enableSlaWhenSecurityContextIsAbsent:true);
            var c106Headers = Result.MessageBus.CommandsSent.Single(x => x is C106v1_LargeCommand).Headers;
            c106Headers.GetIdentityChain().Should().Be($"{AuthSchemePrefixes.Service}://{new TestConfig().AppId}");
            c106Headers.GetIdentityToken().Should().Be(AesOps.Encrypt(new TestConfig().AppId, new TestConfig().EncryptionKey));
            c106Headers.GetAccessToken().Should().BeNullOrEmpty();
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
                    identity: enableSlaWhenSecurityContextIsAbsent ? null : Identities.UserOne.Op(x => x.IdentityPermissions.ApiPermissions.Clear()),
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
