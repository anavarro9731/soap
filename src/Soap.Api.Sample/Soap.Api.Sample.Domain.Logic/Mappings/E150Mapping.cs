namespace Sample.Logic.Mappings
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Sample.Logic.Processes;
    using Sample.Logic.Processes.Stateful;
    using Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class E150Mapping : IMessageFunctionsClientSide<E150Pong>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public Type[] MessageCanContinueTheseStatefulProcesses { get; } =
        {
            typeof(PingAndWaitForPongProcess)
        };

        public Task Handle(E150Pong msg) => throw new NotImplementedException();

        public Task HandleFinalFailure(MessageFailedAllRetries<E150Pong> msg)
        {
            return this.Get<NotifyOfFinalFailureProcess>().Exec(x => x.BeginProcess)(msg);
        }

        public void Validate(E150Pong msg)
        {
        }
    }
}