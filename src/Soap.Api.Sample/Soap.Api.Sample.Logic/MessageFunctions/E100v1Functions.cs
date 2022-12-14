namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class E100v1Functions : IMessageFunctionsClientSide<E100v1_Pong>
    {
        public IContinueProcess<E100v1_Pong>[] HandleWithTheseStatefulProcesses { get; } =
        {
            new P200_C103__PingAndWaitForPong()
        };

        public Task Handle(E100v1_Pong msg) => Task.CompletedTask;

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(E100v1_Pong msg)
        {
            msg.Validate();
        }
    }
}
