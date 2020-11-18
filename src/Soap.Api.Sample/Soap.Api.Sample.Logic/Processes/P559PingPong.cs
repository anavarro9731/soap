namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P559PingPong : Process, IBeginProcess<C100v1Ping>
    {
        public Func<C100v1Ping, Task> BeginProcess =>
            async message =>
                {
                
                await Bus.Publish(
                    new E150Pong
                    {
                        PingedAt = message.PingedAt,
                        PingedBy = message.PingedBy,
                        PongedAt = DateTime.UtcNow,
                        PongedBy = nameof(P559PingPong)
                    });
                };
    }
}