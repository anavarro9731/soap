﻿namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class S301PingAndWaitForPong : StatefulProcess, IBeginProcess<C103v1_StartPingPong>, IContinueProcess<E100v1_Pong>
    {
        public enum States
        {
            Null = 0,

            SentPing = 1,

            PongDoesNotMatchPing = 2,

            ReceivedPong = 3
        }

        public Func<C103v1_StartPingPong, Task> BeginProcess =>
            async message =>
                {
                var pingCommand = new C100v1_Ping
                {
                    C000_PingedAt = DateTime.UtcNow, C000_PingedBy = nameof(S301PingAndWaitForPong)
                };
                pingCommand.Headers.SetMessageId(Guid.NewGuid());

                await Bus.Send(pingCommand);

                await State.AddState(States.SentPing);
                References.PingId = pingCommand.Headers.GetMessageId();
                };

        public Func<E100v1_Pong, Task> ContinueProcess =>
            async message =>
                {
                if (message.E000_PingReference.ToString() != References.PingId)
                {
                    await State.AddState(States.PongDoesNotMatchPing);
                }
                else
                {
                    await State.AddState(States.ReceivedPong);
                }

                CompleteProcess();
                };
    }
}