//* ##REMOVE-IN-COPY##

namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C111v2Functions : IMessageFunctionsClientSide<C111v2_GetRecentTestData>
    {
        public IContinueProcess<C111v2_GetRecentTestData>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C111v2_GetRecentTestData msg) => this.Get<P210_C111__ReturnRecentTestData>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C111v2_GetRecentTestData msg)
        {
            msg.Validate();
        }
    }
}
