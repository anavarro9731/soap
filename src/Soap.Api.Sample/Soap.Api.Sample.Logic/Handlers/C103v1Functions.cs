namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C103v1Functions : IMessageFunctionsClientSide<C103v1_StartPingPong>
    {

        public IContinueProcess<C103v1_StartPingPong>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C103v1_StartPingPong msg) => this.Get<S301PingAndWaitForPong>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C103v1_StartPingPong msg) => msg.Validate();
    }
}
