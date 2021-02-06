namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C110v1Functions : IMessageFunctionsClientSide<C110v1_GetTestDataById>
    {
        public IContinueProcess<C110v1_GetTestDataById>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C110v1_GetTestDataById msg) => this.Get<P208_C110__ReturnTestDataById>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C110v1_GetTestDataById msg)
        {
            msg.Validate();
        }
    }
}
