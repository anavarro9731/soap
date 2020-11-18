namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C103Mapping : IMessageFunctionsClientSide<C103v1StartPingPong>
    {

        public IContinueProcess<C103v1StartPingPong>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C103v1StartPingPong msg) => this.Get<S888PingAndWaitForPong>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C103v1StartPingPong msg) => new C103v1StartPingPong.C103Validator().ValidateAndThrow(msg);
    }
}