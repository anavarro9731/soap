namespace Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Sample.Messages.Commands;
    using Sample.Messages.Events;
    using Soap.MessagePipeline.ProcessesAndOperations;

    public class P101PingPong : Process, IBeginProcess<C100Ping>
    {
        public Func<C100Ping, Task> BeginProcess =>
            async message =>
                {
                await Bus.Publish(
                    new E150Pong
                    {
                        PingedAt = message.PingedAt,
                        PingedBy = message.PingedBy,
                        PongedAt = DateTime.UtcNow,
                        PongedBy = nameof(P101PingPong)
                    });
                };
    }
}