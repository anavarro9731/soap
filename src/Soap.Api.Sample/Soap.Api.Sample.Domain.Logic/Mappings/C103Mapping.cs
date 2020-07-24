namespace Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Sample.Logic.Processes;
    using Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.MessagePipeline.ProcessesAndOperations;
    using Soap.Utility.Objects.Blended;

    public class C103Mapping : IMessageFunctionsClientSide<C103StartPingPong>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public IContinueProcess<C103StartPingPong>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C103StartPingPong msg) => this.Get<S100PingAndWaitForPong>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C103StartPingPong> msg) =>
            this.Get<P103NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C103StartPingPong msg) => new C103StartPingPong.C103Validator().ValidateAndThrow(msg);
    }
}