namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C114v1Functions : IMessageFunctionsClientSide<C114v1_DeleteTestDataById>
    {
        public IContinueProcess<C114v1_DeleteTestDataById>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C114v1_DeleteTestDataById msg)
        {
            return this.Get<P213_C114__RemoveTestData>().Call(x => x.BeginProcess)(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries msg)
        {
            return this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);
        }

        public void Validate(C114v1_DeleteTestDataById msg)
        {
            msg.Validate();
        }
    }
}