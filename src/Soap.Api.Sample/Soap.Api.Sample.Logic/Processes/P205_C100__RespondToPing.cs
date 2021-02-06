namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P205_C100__RespondToPing : Process, IBeginProcess<C100v1_Ping>
    {
        public Func<C100v1_Ping, Task> BeginProcess =>
            async message =>
                {
                await PublishPong();

                async Task PublishPong()
                {
                    var p = Meta.UserProfileOrNull;

                    await Bus.Publish(
                        new E100v1_Pong
                        {
                            E000_PingedAt = message.C000_PingedAt,
                            E000_PingedBy = message.C000_PingedBy,
                            E000_PongedAt = DateTime.UtcNow,
                            E000_PongedBy = p?.Auth0Id
                        });
                }
                };
    }
}
