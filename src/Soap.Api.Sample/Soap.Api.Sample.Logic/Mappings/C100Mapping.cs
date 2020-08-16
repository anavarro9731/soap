namespace Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Blended;

    public class C100Mapping : IMessageFunctionsClientSide<C100Ping>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public IContinueProcess<C100Ping>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C100Ping msg) => this.Get<P559PingPong>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C100Ping> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C100Ping msg)
        {
        }
    }
}