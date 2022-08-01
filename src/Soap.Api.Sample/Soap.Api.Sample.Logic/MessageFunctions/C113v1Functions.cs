//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C113v1Functions : IMessageFunctionsClientSide<C113v1_GetC107FormDataForEdit>
    {
        public IContinueProcess<C113v1_GetC107FormDataForEdit>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C113v1_GetC107FormDataForEdit msg) => this.Get<P212_C113__ReturnC107FormDataForEdit>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C113v1_GetC107FormDataForEdit msg)
        {
            msg.Validate();
        }
    }
}
