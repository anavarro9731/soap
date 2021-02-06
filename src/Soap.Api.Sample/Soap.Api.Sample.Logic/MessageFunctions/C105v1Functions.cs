namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C105v1Functions : IMessageFunctionsClientSide<C105v1_SendLargeMessage>
    {

        public IContinueProcess<C105v1_SendLargeMessage>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C105v1_SendLargeMessage msg) => this.Get<P206_C105__SendLargeMessage>().Call(s => s.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C105v1_SendLargeMessage msg)
        {
            msg.Validate();
        }
    }
}
