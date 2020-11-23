namespace Soap.Api.Sample.Tests.StatefulProcesses
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces.Messages;
    using Soap.Utility.Objects.Binary;
    using Xunit;
    using Xunit.Abstractions;

    public class S100Test : Test
    {
        public S100Test(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void ErrorPongDoesntMatchPing()
        {
            SendC103();

            Result.ActiveProcessState.EnumFlags.HasFlag(S888PingAndWaitForPong.States.SentPing).Should().BeTrue();
            var ping = Result.MessageBus.CommandsSent.Single(x => x.GetType() == typeof(C100v1Ping));
            ping.Headers.GetStatefulProcessId().Should().NotBeNull();

            SendE150();

            Result.ActiveProcessState.EnumFlags.HasFlag(S888PingAndWaitForPong.States.PongDoesNotMatchPing).Should().BeTrue();
            Result.ActiveProcessState.EnumFlags.HasFlag(S888PingAndWaitForPong.States.ReceivedPong).Should().BeFalse();

            void SendC103()
            {
                var c103StartPingPong = new C103v1StartPingPong();
                SetupTestByProcessingAMessage(c103StartPingPong, Identities.UserOne);
            }

            void SendE150()
            {
                var pong = new E150v1Pong
                {
                    PingReference = Guid.NewGuid(), PingedAt = ping.Headers.GetTimeOfCreationAtOrigin()
                };
                pong.Headers.SetStatefulProcessId(ping.Headers.GetStatefulProcessId().Value);
                SetupTestByProcessingAMessage(pong, Identities.UserOne);
            }
        }

        [Fact]
        public void HappyPath()
        {
            SendC103();

            Result.ActiveProcessState.EnumFlags.HasFlag(S888PingAndWaitForPong.States.SentPing).Should().BeTrue();
            var ping = Result.MessageBus.CommandsSent.Single(x => x.GetType() == typeof(C100v1Ping));
            ping.Headers.GetStatefulProcessId().Should().NotBeNull();

            SendE150();

            Result.ActiveProcessState.EnumFlags.HasFlag(S888PingAndWaitForPong.States.ReceivedPong).Should().BeTrue();

            void SendC103()
            {
                var c103StartPingPong = new C103v1StartPingPong();
                SetupTestByProcessingAMessage(c103StartPingPong, Identities.UserOne);
            }

            void SendE150()
            {
                var pong = new E150v1Pong
                {
                    PingReference = ping.Headers.GetMessageId(), PingedAt = ping.Headers.GetTimeOfCreationAtOrigin()
                };
                pong.Headers.SetStatefulProcessId(ping.Headers.GetStatefulProcessId().Value);
                SetupTestByProcessingAMessage(pong, Identities.UserOne);
            }
        }

        [Fact]
        public void StatefulProcessIdNotPresentOnPong()
        {
            SendC103();

            Result.ActiveProcessState.EnumFlags.HasFlag(S888PingAndWaitForPong.States.SentPing).Should().BeTrue();
            var ping = Result.MessageBus.CommandsSent.Single(x => x.GetType() == typeof(C100v1Ping));
            ping.Headers.GetStatefulProcessId().Should().NotBeNull();

            SendE150();

            Result.ActiveProcessState.Should().BeNull();

            void SendC103()
            {
                var c103StartPingPong = new C103v1StartPingPong();
                SetupTestByProcessingAMessage(c103StartPingPong, Identities.UserOne);
            }

            void SendE150()
            {
                var pong = new E150v1Pong
                {
                    PingReference = Guid.NewGuid(), PingedAt = ping.Headers.GetTimeOfCreationAtOrigin()
                };
                SetupTestByProcessingAMessage(pong, Identities.UserOne);
            }
        }
    }
}