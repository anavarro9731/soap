namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class E150v1Functions : IMessageFunctionsClientSide<E150v1_Pong>
    {
        public IContinueProcess<E150v1_Pong>[] HandleWithTheseStatefulProcesses { get; } =
        {
            new S888PingAndWaitForPong()
        };

        public Task Handle(E150v1_Pong msg) => Task.CompletedTask;

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(E150v1_Pong msg)
        {
            msg.Validate();
        }
    }
}
