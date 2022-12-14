namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C100v1Functions : IMessageFunctionsClientSide<C100v1_Ping>
    {
        public IContinueProcess<C100v1_Ping>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C100v1_Ping msg) => this.Get<P205_C100__RespondToPing>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C100v1_Ping msg)
        {
        }
    }
}
