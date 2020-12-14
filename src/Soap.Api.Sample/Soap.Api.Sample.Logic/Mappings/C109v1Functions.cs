namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C109v1Functions : IMessageFunctionsClientSide<C109v1_GetForm>
    {
        public IContinueProcess<C109v1_GetForm>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C109v1_GetForm msg) => this.Get<P561ReturnForm>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C109v1_GetForm msg)
        {
            msg.Validate();
        }
    }
}
