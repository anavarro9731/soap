namespace Sample.Logic.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Sample.Logic.Processes;
    using Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Blended;

    public class E150Mapping : IMessageFunctionsClientSide<E150Pong>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public IContinueProcess<E150Pong>[] HandleWithTheseStatefulProcesses { get; } =
        {
            new S100PingAndWaitForPong()
        };

        public Task Handle(E150Pong msg) => Task.CompletedTask;

        public Task HandleFinalFailure(MessageFailedAllRetries<E150Pong> msg) => this.Get<P103NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(E150Pong msg)
        {
        }
    }
}