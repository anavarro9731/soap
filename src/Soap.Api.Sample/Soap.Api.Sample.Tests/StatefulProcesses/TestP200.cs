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

    public class P200Test : Test
    {
        public P200Test(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public void ErrorPongDoesntMatchPing()
        {
            SendC103();

            Result.ActiveProcessState.EnumFlags.HasFlag(P200_C103__PingAndWaitForPong.States.SentPing).Should().BeTrue();
            var ping = Result.MessageBus.CommandsSent.Single(x => x.GetType() == typeof(C100v1_Ping));
            ping.Headers.GetStatefulProcessId().Should().NotBeNull();

            SendE150();

            Result.ActiveProcessState.EnumFlags.HasFlag(P200_C103__PingAndWaitForPong.States.PongDoesNotMatchPing).Should().BeTrue();
            Result.ActiveProcessState.EnumFlags.HasFlag(P200_C103__PingAndWaitForPong.States.ReceivedPong).Should().BeFalse();

            void SendC103()
            {
                var c103StartPingPong = new C103v1_StartPingPong();
                SetupTestByProcessingAMessage(c103StartPingPong, Identities.JohnDoeAllPermissions);
            }

            void SendE150()
            {
                var pong = new E100v1_Pong
                {
                    E000_PingReference = Guid.NewGuid(), E000_PingedAt = ping.Headers.GetTimeOfCreationAtOrigin()
                };
                pong.Headers.SetStatefulProcessId(ping.Headers.GetStatefulProcessId().Value);
                SetupTestByProcessingAMessage(pong, Identities.JohnDoeAllPermissions);
            }
        }

        [Fact]
        public void HappyPath()
        {
            SendC103();

            Result.ActiveProcessState.EnumFlags.HasFlag(P200_C103__PingAndWaitForPong.States.SentPing).Should().BeTrue();
            var ping = Result.MessageBus.CommandsSent.Single(x => x.GetType() == typeof(C100v1_Ping));
            ping.Headers.GetStatefulProcessId().Should().NotBeNull();

            SendE150();

            Result.ActiveProcessState.EnumFlags.HasFlag(P200_C103__PingAndWaitForPong.States.ReceivedPong).Should().BeTrue();

            void SendC103()
            {
                var c103StartPingPong = new C103v1_StartPingPong();
                SetupTestByProcessingAMessage(c103StartPingPong, Identities.JohnDoeAllPermissions);
            }

            void SendE150()
            {
                var pong = new E100v1_Pong
                {
                    E000_PingReference = ping.Headers.GetMessageId(), E000_PingedAt = ping.Headers.GetTimeOfCreationAtOrigin()
                };
                pong.Headers.SetStatefulProcessId(ping.Headers.GetStatefulProcessId().Value);
                SetupTestByProcessingAMessage(pong, Identities.JohnDoeAllPermissions);
            }
        }

        [Fact]
        public void StatefulProcessIdNotPresentOnPong()
        {
            SendC103();

            Result.ActiveProcessState.EnumFlags.HasFlag(P200_C103__PingAndWaitForPong.States.SentPing).Should().BeTrue();
            var ping = Result.MessageBus.CommandsSent.Single(x => x.GetType() == typeof(C100v1_Ping));
            ping.Headers.GetStatefulProcessId().Should().NotBeNull();

            SendE150();

            Result.ActiveProcessState.Should().BeNull();

            void SendC103()
            {
                var c103StartPingPong = new C103v1_StartPingPong();
                SetupTestByProcessingAMessage(c103StartPingPong, Identities.JohnDoeAllPermissions);
            }

            void SendE150()
            {
                var pong = new E100v1_Pong
                {
                    E000_PingReference = Guid.NewGuid(), E000_PingedAt = ping.Headers.GetTimeOfCreationAtOrigin()
                };
                SetupTestByProcessingAMessage(pong, Identities.JohnDoeAllPermissions);
            }
        }
    }
}
