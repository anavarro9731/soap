namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C107Mapping : IMessageFunctionsClientSide<C107v1_TestDataTypes>
    {
        public IContinueProcess<C107v1_TestDataTypes>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C107v1_TestDataTypes msg) => Task.CompletedTask;

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C107v1_TestDataTypes msg)
        {
        }
    }
}
