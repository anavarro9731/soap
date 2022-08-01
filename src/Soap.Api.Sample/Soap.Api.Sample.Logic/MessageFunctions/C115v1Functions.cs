
namespace Soap.Api.Sample.Logic.MessageFunctions
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C115v1Functions : IMessageFunctionsClientSide<C115v1_OnStartup>
    {
        public IContinueProcess<C115v1_OnStartup>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C115v1_OnStartup msg)
        {
            return this.Get<P214_C115__HandleOnStartup>().Call(x => x.BeginProcess)(msg);
        }

        public Task HandleFinalFailure(MessageFailedAllRetries msg)
        {
            return this.Get<P203_MessageFailedAllRetries__NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);
        }

        public void Validate(C115v1_OnStartup msg)
        {
            msg.Validate();
        }
    }
}