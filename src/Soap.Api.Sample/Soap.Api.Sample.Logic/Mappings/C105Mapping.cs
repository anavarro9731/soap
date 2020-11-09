//*     ##REMOVE-IN-COPY##
namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C105Mapping : IMessageFunctionsClientSide<C105SendLargeMessage>
    {

        public IContinueProcess<C105SendLargeMessage>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C105SendLargeMessage msg) => this.Get<P560SendLargeMessage>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C105SendLargeMessage> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C105SendLargeMessage msg)
        {
        }
    }
}