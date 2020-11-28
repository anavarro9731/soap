namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C100Mapping : IMessageFunctionsClientSide<C100v1_Ping>
    {
        public IContinueProcess<C100v1_Ping>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C100v1_Ping msg) => this.Get<P559PingPong>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C100v1_Ping msg)
        {
        }
    }
}