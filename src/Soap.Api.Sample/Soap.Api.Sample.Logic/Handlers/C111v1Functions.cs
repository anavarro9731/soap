namespace Soap.Api.Sample.Logic.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C111v1Functions : IMessageFunctionsClientSide<C111v1_GetRecentTestData>
    {
        public IContinueProcess<C111v1_GetRecentTestData>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C111v1_GetRecentTestData msg) => this.Get<P210ReturnRecentTestData>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C111v1_GetRecentTestData msg)
        {
            msg.Validate();
        }
    }
}
