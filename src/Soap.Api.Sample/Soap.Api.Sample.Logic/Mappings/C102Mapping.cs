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

    public class C102Mapping : IMessageFunctionsClientSide<C102GetServiceState>
    {
        public Dictionary<ErrorCode, ErrorCode> GetErrorCodeMapper { get; }

        public IContinueProcess<C102GetServiceState>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C102GetServiceState msg) => this.Get<P556GetServiceState>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C102GetServiceState> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C102GetServiceState msg)
        {
            new C102GetServiceState.Validator().ValidateAndThrow(msg);
        }
    }
}