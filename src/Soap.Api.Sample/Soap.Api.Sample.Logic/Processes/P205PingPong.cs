namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P205PingPong : Process, IBeginProcess<C100v1_Ping>
    {
        public Func<C100v1_Ping, Task> BeginProcess =>
            async message =>
                {
                
                await Publish(
                    new E100v1_Pong
                    {
                        C000_PingedAt = message.C000_PingedAt,
                        C000_PingedBy = message.C000_PingedBy,
                        C000_PongedAt = DateTime.UtcNow,
                        C000_PongedBy = nameof(P205PingPong)
                    });
                };
    }
}
