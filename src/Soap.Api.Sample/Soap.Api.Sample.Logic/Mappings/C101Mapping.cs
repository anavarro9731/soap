namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Utility.Objects.Blended;

    public class C101Mapping : IMessageFunctionsClientSide<C101UpgradeTheDatabase>
    {
        public IContinueProcess<C101UpgradeTheDatabase>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C101UpgradeTheDatabase msg) => this.Get<P558UpgradeTheDatabase>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries<C101UpgradeTheDatabase> msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C101UpgradeTheDatabase msg)
        {
            new C101UpgradeTheDatabase.Validator().ValidateAndThrow(msg);
        }
    }
}