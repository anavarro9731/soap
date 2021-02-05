//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C112v1Functions : IMessageFunctionsClientSide<C112v1_MessageThatDoesntRequireAuthorisation>
    {
        public IContinueProcess<C112v1_MessageThatDoesntRequireAuthorisation>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C112v1_MessageThatDoesntRequireAuthorisation msg) => this.Get<P211RespondToMessageThatDoesntRequireAuthorisation>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C112v1_MessageThatDoesntRequireAuthorisation msg)
        {
            msg.Validate();
        }
    }
}
