namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C109v1Functions : IMessageFunctionsClientSide<C109v1_GetC107FormData>
    {
        public IContinueProcess<C109v1_GetC107FormData>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C109v1_GetC107FormData msg) => this.Get<P207_C109__ReturnC107FormData>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C109v1_GetC107FormData msg)
        {
            msg.Validate();
        }
    }
}
