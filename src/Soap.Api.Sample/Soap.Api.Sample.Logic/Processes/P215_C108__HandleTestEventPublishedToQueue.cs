namespace Soap.Api.Sample.Logic.Processes
{
    using System;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;
    using Soap.PfBase.Logic.ProcessesAndOperations;

    public class P215_C108__HandleTestEventPublishedToQueue : Process, IBeginProcess<C108v1_TestEventPublishedToQueue>
    {
        public Func<C108v1_TestEventPublishedToQueue, Task> BeginProcess =>
            async message =>
                {
                var e100V1Pong = new E100v1_Pong();
                e100V1Pong.Headers.SetQueueName("test-queue");

                var eventVisibilityFlags = message.C108_SetQueueHeaderOnly.GetValueOrDefault()
                                               ? null
                                               : new IBusClient.EventVisibilityFlags(IBusClient.EventVisibility.SendDirectToQueue);

                await Bus.Publish(e100V1Pong, eventVisibilityFlags);
                };
    }
}