namespace Soap.Api.Sample.Logic.Handlers
{
    using System.Threading.Tasks;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C101v1Functions : IMessageFunctionsClientSide<C101v1_UpgradeTheDatabase>
    {
        public IContinueProcess<C101v1_UpgradeTheDatabase>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C101v1_UpgradeTheDatabase msg) => this.Get<P204UpgradeTheDatabase>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P203NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C101v1_UpgradeTheDatabase msg)
        {
            msg.Validate();
            
        }
    }
}
