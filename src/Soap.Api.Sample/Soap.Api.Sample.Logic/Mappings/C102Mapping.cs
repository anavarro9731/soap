namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C102Mapping : IMessageFunctionsClientSide<C102v1_GetServiceState>
    {

        public IContinueProcess<C102v1_GetServiceState>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C102v1_GetServiceState msg) => this.Get<P556GetServiceState>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C102v1_GetServiceState msg)
        {
            new C102v1_GetServiceState.Validator().ValidateAndThrow(msg);
        }
    }
}