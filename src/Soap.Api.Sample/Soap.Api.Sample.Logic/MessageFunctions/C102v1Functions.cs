
namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C102v1Functions : IMessageFunctionsClientSide<C102v1_GetServiceState>
    {
        public IContinueProcess<C102v1_GetServiceState>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C102v1_GetServiceState msg) => this.Get<P202_C102__GetServiceState>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C102v1_GetServiceState msg)
        {
            msg.Validate();
        }
    }
}
