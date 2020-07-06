namespace Sample.Logic.Processes.Stateful
{
    using System;
    using System.Threading.Tasks;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Blended;

    public class PingAndWaitForPongProcess : StatefulProcess, IBeginProcess<C100Ping>, IContinueProcess<E150Pong>
    {
        public enum States
        {
            Null = 0,

            SentC100 = 1,

            ReceivedE150 = 2
        }

        public Func<C100Ping, Task> BeginProcess =>
            async message =>
                {
                await Bus.Publish(
                    new E150Pong
                    {
                        PingedAt = message.PingedAt,
                        PingedBy = message.PingedBy,
                        PongedAt = DateTime.UtcNow,
                        PongedBy = nameof(PingPongProcess)
                    });
                await State.AddState(States.SentC100);
                References.Whatever = "whatever";
                };

        public Func<E150Pong, Task> ContinueProcess => async message => { await State.AddState(States.ReceivedE150); };

    }
}