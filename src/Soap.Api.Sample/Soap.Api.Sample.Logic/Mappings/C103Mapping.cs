namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C103Mapping : IMessageFunctionsClientSide<C103StartPingPong>
    {

        public IContinueProcess<C103StartPingPong>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C103StartPingPong msg) => this.Get<S888PingAndWaitForPong>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C103StartPingPong> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C103StartPingPong msg) => new C103StartPingPong.C103Validator().ValidateAndThrow(msg);
    }
}