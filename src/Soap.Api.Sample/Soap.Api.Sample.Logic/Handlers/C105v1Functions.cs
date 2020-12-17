//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C105v1Functions : IMessageFunctionsClientSide<C105v1_SendLargeMessage>
    {

        public IContinueProcess<C105v1_SendLargeMessage>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C105v1_SendLargeMessage msg) => this.Get<P206SendLargeMessage>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C105v1_SendLargeMessage msg)
        {
            msg.Validate();
        }
    }
}
