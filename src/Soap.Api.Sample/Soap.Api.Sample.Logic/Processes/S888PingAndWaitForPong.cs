namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;
    using Soap.Utility.Functions.Extensions;

    public class S888PingAndWaitForPong : StatefulProcess, IBeginProcess<C103v1StartPingPong>, IContinueProcess<E150Pong>
    {
        public enum States
        {
            Null = 0,

            SentPing = 1,

            PongDoesNotMatchPing = 2,

            ReceivedPong = 3
        }

        public Func<C103v1StartPingPong, Task> BeginProcess =>
            async message =>
                {
                var pingCommand = new C100v1Ping
                {
                    PingedAt = DateTime.UtcNow, PingedBy = nameof(S888PingAndWaitForPong)
                };
                pingCommand.Headers.SetMessageId(Guid.NewGuid());

                await Bus.Send(pingCommand);

                await State.AddState(States.SentPing);
                References.PingId = pingCommand.Headers.GetMessageId();
                };

        public Func<E150Pong, Task> ContinueProcess =>
            async message =>
                {
                if (message.PingReference.ToString() != References.PingId)
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