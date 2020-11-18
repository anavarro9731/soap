namespace Soap.Api.Sample.Logic.Mappings
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentValidation;
    using Soap.Api.Sample.Logic.Operations;
    using Soap.Api.Sample.Logic.Processes;
    using Soap.Api.Sample.Messages.Commands;
    using Soap.Interfaces;
    using Soap.Interfaces.Messages;

    public class C101Mapping : IMessageFunctionsClientSide<C101v1UpgradeTheDatabase>
    {
        public IContinueProcess<C101v1UpgradeTheDatabase>[] HandleWithTheseStatefulProcesses { get; }

        public Task Handle(C101v1UpgradeTheDatabase msg) => this.Get<P558UpgradeTheDatabase>().Call(x => x.BeginProcess)(msg);

        public Task HandleFinalFailure(MessageFailedAllRetries msg) =>
            this.Get<P557NotifyOfFinalFailure>().Call(x => x.BeginProcess)(msg);

        public void Validate(C101v1UpgradeTheDatabase msg)
        {
            new C101v1UpgradeTheDatabase.Validator().ValidateAndThrow(msg);
        }
    }
}