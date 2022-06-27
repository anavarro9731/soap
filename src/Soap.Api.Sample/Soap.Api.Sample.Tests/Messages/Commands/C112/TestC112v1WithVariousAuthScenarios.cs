//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Tests.Messages.Commands.C112
{
    using System.Linq;
    using CircuitBoard;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Context;
    using Soap.Idaam;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Tests;
    using Soap.Utility.Functions.Extensions;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC112v1WithVariousAuthScenarios : Test
    {
        public TestC112v1WithVariousAuthScenarios(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        [Fact]
        public void ItShouldReuseIncomingAuthHeadersOnOutgoingMessageByDefaultEvenWhenTheIncomingMessageDoesntRequireThem()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesRequireAuthorisation, Identities.JaneDoeNoPermissions);
            var outgoingHeaders = Result.MessageBus.CommandsSent.Single(x => x is C100v1_Ping).Headers;
            outgoingHeaders.GetIdentityChain().Should().Be(Identities.JaneDoeNoPermissions.IdChainSegment);
            outgoingHeaders.GetIdentityToken().Should().Be(Identities.JaneDoeNoPermissions.IdToken(new TestConfig().EncryptionKey));
            outgoingHeaders.GetAccessToken().Should().Be(Identities.JaneDoeNoPermissions.AccessToken);
        }
        
        [Fact]
        public void ItShouldSetSlaHeadersOnTheOutgoingMessageIfServiceLevelAuthIsRequestedByTheSenderOfTheOutgoingMessage()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesRequireAuthorisation, Identities.JaneDoeNoPermissions, forceServiceLevelAuthOnOutgoingC106:true);
            var serviceLevelAuthority = (AuthorisationSchemes.GetServiceLevelAuthority(new TestConfig()));
            var outgoingHeaders = Result.MessageBus.CommandsSent.Single(x => x is C100v1_Ping).Headers;
            outgoingHeaders.GetIdentityChain().Should().Be($"{Identities.JaneDoeNoPermissions.IdChainSegment},{serviceLevelAuthority.IdentityChainSegment}");
            outgoingHeaders.GetIdentityToken().Should().Be(serviceLevelAuthority.IdentityToken);
            outgoingHeaders.GetAccessToken().Should().Be(serviceLevelAuthority.AccessToken);
        }
        
        [Fact]
        public void ItShouldSetSlaHeadersOnOutgoingMessageWhenThereIsNoAuthHeadersOnIncomingButEnableSlaWhenSecurityContextIsAbsentIsSet()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesRequireAuthorisation, null, enableSlaWhenSecurityContextIsAbsent:true);
            var serviceLevelAuthority = (AuthorisationSchemes.GetServiceLevelAuthority(new TestConfig()));
            var outgoingHeaders = Result.MessageBus.CommandsSent.Single(x => x is C100v1_Ping).Headers;
            outgoingHeaders.GetIdentityChain().Should().Be($"{serviceLevelAuthority.IdentityChainSegment}");
            outgoingHeaders.GetIdentityToken().Should().Be(serviceLevelAuthority.IdentityToken);
            outgoingHeaders.GetAccessToken().Should().Be(serviceLevelAuthority.AccessToken);
        }

        [Fact]
        public void
            ItShouldSetNoAuthHeadersOnOutgoingMessageAndFailWhenThereIsNoAuthHeadersOnIncomingAndEnableSlaWhenSecurityContextIsAbsentIsNotSet()
        {
            Setup(
                C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesRequireAuthorisation,
                null);
            Result.Success.Should().BeFalse();
        }

        [Fact]
        public void ItShouldSetNoAuthHeadersOnTheOutgoingMessageIfAuthIsDisabledAndSucceedEvenWhenTheOutgoingMessageRequiresAuth()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesRequireAuthorisation, Identities.JohnDoeAllPermissions, disableAuth: true);
            var outgoingHeaders = Result.MessageBus.CommandsSent.Single(x => x is C100v1_Ping).Headers;
            outgoingHeaders.GetIdentityChain().Should().BeNullOrEmpty();
            outgoingHeaders.GetIdentityToken().Should().BeNullOrEmpty();
            outgoingHeaders.GetAccessToken().Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void ItShouldSetNoAuthHeadersOnTheOutgoingMessageIfTheIncomingDoesntHaveThemAndSucceedIfTheOutgoingMessageDoesntRequireAuth()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesntRequireAuthorisation, null);
            var outgoingHeaders = Result.MessageBus.CommandsSent.Single(x => x is C112v1_MessageThatDoesntRequireAuthorisation).Headers;
            outgoingHeaders.GetIdentityChain().Should().BeNullOrEmpty();
            outgoingHeaders.GetIdentityToken().Should().BeNullOrEmpty();
            outgoingHeaders.GetAccessToken().Should().BeNullOrEmpty();
        }
        
        [Fact]
        public void ItShouldSetNoAuthHeadersOnTheOutgoingMessageIfTheIncomingDoesntHaveThemAndFailIfTheOutgoingMessageRequiresAuth()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.SendAnotherCommandThatDoesRequireAuthorisation, null);
            Result.Success.Should().BeFalse();
        }
        
        [Fact]
        public void ItShouldSucceedWithAuthHeadersButNoPermissionsWhenTheIncomingMessageHasTheNoAuthAttribute()
        {
            Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction.DoNothing, Identities.JaneDoeNoPermissions);
            Result.Success.Should().BeTrue();
        }

        private void Setup(C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction forwardAction, TestIdentity testIdentity, bool forceServiceLevelAuthOnOutgoingC106 = false, bool disableAuth = false, bool enableSlaWhenSecurityContextIsAbsent = false)
        {
            TestMessage(
                    new C112v1_MessageThatDoesntRequireAuthorisation().Op(
                        m =>
                            {
                            if (forceServiceLevelAuthOnOutgoingC106)
                            {
                                m.Headers.SetMessageId(SpecialIds.ForceServiceLevelAuthorityOnOutgoingMessages);
                            }

                            m.C112_NextAction = new TypedEnumerationAndFlags<C112v1_MessageThatDoesntRequireAuthorisation.ForwardAction>(forwardAction);
                            }),
                    identity: testIdentity,
                    enableSlaWhenSecurityContextIsMissing: enableSlaWhenSecurityContextIsAbsent,
                    authLevel: disableAuth ? AuthLevel.None : AuthLevel.ApiAndDatabasePermission).Wait();
        }
    }
}
