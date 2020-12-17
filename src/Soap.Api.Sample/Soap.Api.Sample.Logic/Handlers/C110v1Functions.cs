namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C110v1Functions : IMessageFunctionsClientSide<C110v1_GetTestData>
    {
        public IContinueProcess<C110v1_GetTestData>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C110v1_GetTestData msg) => this.Get<P208ReturnTestData>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C110v1_GetTestData msg)
        {
            msg.Validate();
        }
    }
}
