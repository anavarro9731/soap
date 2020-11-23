﻿namespace Soap.Api.Sample.Tests.Messages
{
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Messages.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class TestC102 : Test
    {
        public TestC102(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
            SetupTestByProcessingAMessage(Commands.UpgradeTheDatabaseToV1, Identities.UserOne);
            SetupTestByProcessingAMessage(Commands.GetServiceState, Identities.UserOne);
        }

        [Fact]
        public void ItShouldSendAGotMessageEvent()
        {
            Result.MessageBus.EventsPublished.Should().ContainSingle();
            Result.MessageBus.EventsPublished.Single().Should().BeOfType<E151v1GotServiceState>();
        }
    }
}