namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Events;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class E150Mapping : IMessageFunctionsClientSide<E150Pong>
    {

        public IContinueProcess<E150Pong>[] HandleWithTheseStatefulProcesses { get; } =
        {
            new S888PingAndWaitForPong()
        };

        public Task Handle(E150Pong msg) => Task.CompletedTask;

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(E150Pong msg)
        {
        }
    }
}